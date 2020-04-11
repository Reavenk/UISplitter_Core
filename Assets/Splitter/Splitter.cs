using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIUtils
    { 
        [RequireComponent(typeof(RectTransform))]
        public class Splitter : MonoBehaviour
        {
            public enum SplitGrain
            { 
                Horizontal,
                Vertical
            }

            private struct SashKey
            { 
                RectTransform a;    // Top or left
                RectTransform b;    // Bottom or right

                public SashKey(RectTransform a, RectTransform b)
                { 
                    this.a = a;
                    this.b = b;
                }
            }

            public struct Sash
            { 
                public UnityEngine.UI.Image sash;
            }

            private RectTransform rectTransform;

            private Dictionary<RectTransform, float>  rectSizes = new Dictionary<RectTransform, float>();
            private Dictionary<SashKey, UnityEngine.UI.Image> sashes = null;

            public float minSize = 50.0f;

            public SplitGrain splitGrain = SplitGrain.Horizontal;

            public SplitterProps props;
            public List<RectTransform> panes;

            void Awake()
            { 
                this.rectTransform = this.GetComponent<RectTransform>();
                int idx = 0;
                if(this.splitGrain == SplitGrain.Vertical)
                    idx = 1;

                if(this.panes != null)
                { 
                    foreach(RectTransform rt in this.panes)
                    {
                        Vector2 origSz = rt.rect.size;
                        PrepareAsChild(rt);
                        this.rectSizes.Add(rt, origSz[idx]);

                    }
                }
                this.UpdateAlignment();

            }

            public int GetVectorIndex()
            { 
                if(this.splitGrain == SplitGrain.Vertical)
                    return 1;

                return  0;
            }

            public void GetVectorIndices(out int main, out int other)
            { 
                if(this.splitGrain == SplitGrain.Vertical)
                { 
                    main = 1;
                    other = 0;
                }

                main = 0;
                other = 1;
            }

            private void PrepareAsChild(RectTransform tr)
            {
                tr.transform.SetParent(this.transform);
                tr.localPosition    = Vector3.zero;
                tr.localRotation    = Quaternion.identity;
                tr.localScale       = Vector3.one;

                tr.pivot = new Vector2(0.0f, 1.0f);
                tr.anchorMin = new Vector2(0.0f, 1.0f);
                tr.anchorMax = new Vector2(0.0f, 1.0f);
                tr.offsetMin = new Vector2(0.0f, 0.0f);
                tr.offsetMax = new Vector2(0.0f, 0.0f);
            }

            private void ClearSashes()
            { 
                // Destroy them all
                foreach(KeyValuePair<SashKey, UnityEngine.UI.Image> kvp in this.sashes)
                    GameObject.Destroy(kvp.Value.gameObject);

                // We null it instead of clearing it out, because
                // clearing it means the sashes are up-to-date and there
                // just are none;
                // Where nulling it means it needs to be reconstructed if encountered
                // at a later time.
                this.sashes = null;
            }

            private bool RemakeSahes()
            { 
                if(this.sashes != null)
                    return false;

                this.sashes = new Dictionary<SashKey, UnityEngine.UI.Image>();
                for(int i = 0; i < this.panes.Count - 1; ++i)
                { 
                    SashKey sk = new SashKey(this.panes[i], this.panes[i + 1]);

                    GameObject goSplitter = new GameObject("_Splitter");
                    SplitterSash imgSplitter = goSplitter.AddComponent<SplitterSash>();
                    goSplitter.transform.SetParent(this.transform);
                    imgSplitter.sprite = this.props.spriteHoriz;
                    imgSplitter.type = UnityEngine.UI.Image.Type.Sliced;
                    this.PrepareAsChild(imgSplitter.rectTransform);

                    imgSplitter.paneA = this.panes[i];
                    imgSplitter.paneB = this.panes[i + 1];
                    imgSplitter.parent = this;

                    this.sashes.Add(sk, imgSplitter);
                }

                return true;
            }

            public void UpdateAlignment()
            { 
                if(this.rectTransform == null)
                    return;

                Rect trect = this.rectTransform.rect;
                Vector2 thisDim = trect.size;

                if(this.rectSizes.Count == 0)
                    return;

                this.RemakeSahes();

                if(this.rectSizes.Count == 1)
                { 
                    RectTransform rt = null;
                    foreach(KeyValuePair< RectTransform, float> kvp in this.rectSizes)
                        rt = kvp.Key;

                    rt.anchoredPosition = new Vector2(0.0f, 0.0f);
                    rt.sizeDelta = thisDim;
                    return;
                }
                
                // Set idx to 0 to reference the x component (horizontal) in vectors.
                // Set idx to 1 to reference the y component (vertical) in vectors.
                int idx = 0;
                if(this.splitGrain == SplitGrain.Vertical)
                    idx = 1;

                // The component for this splitter container's dimensions that we care about.
                float availTotalSize = thisDim[idx];
                float sashSpace = (this.rectSizes.Count - 1) * this.props.sashDim[idx];
                // 
                float availWOSashes = availTotalSize - sashSpace;

                // The total amount needed for the minimum
                float totalMin = this.rectSizes.Count * this.minSize;
                float totalMinWSashes = totalMin + sashSpace;

                // How much extra space can be distributed
                float distribSpace = availWOSashes - totalMin;

                float total = 0.0f;


                // If we don't have enough space to fit to even fit
                // everything and their min size, nothing gets to decide
                // on what portion of excess space (there there is none of)
                // to take. We leave total at 0 so equal space handling will
                // happen.
                if(availWOSashes > totalMin)
                {
                    float excess = 0.0f;

                    // Figure out how much excess space there is to allocate after
                    // we make sure everything follows the constraint for the 
                    // min size.
                    foreach (RectTransform rtKey in this.panes)
                    {
                        float size = this.rectSizes[rtKey];
                        if(size < this.minSize)
                        { 
                            this.rectSizes[rtKey] = this.minSize;
                            size = this.minSize;
                        }
                        else
                            excess += size - this.minSize; 

                        total += size;
                    }

                    // Divy up the space based off any changes that might have been
                    // made to allow for the min size constraint
                    if (excess > 0.0f)
                    {
                        foreach(RectTransform rtKey in this.panes)
                        { 
                            float sz = this.rectSizes[rtKey];
                            if(sz > this.minSize)
                            {
                                float redistSize = 
                                    this.minSize + (sz - this.minSize) / excess * distribSpace;

                                this.rectSizes[rtKey] = redistSize;
                                    
                            }
                        }
                    }
                }

                if(total == 0.0f)
                { 
                    // Some weird collapsed state, or starting state.
                    // Just make everything equal
                    //
                    // We can't use the minsize, because we're most likely here because
                    // we didn't even have the space for minsize.
                    float avgSize = availWOSashes / total;
                    foreach(RectTransform rt in this.rectSizes.Keys)
                        this.rectSizes[rt] = avgSize;
                }
                else
                { 
                    int other = (idx == 0) ? 1 : 0;
                    float sign = 1.0f;

                    if(idx == 1)
                        sign = -1.0f;

                    RectTransform lastRt = null;
                    float incr = 0.0f;

                    Vector2 sashDim = Vector2.zero;
                    sashDim[idx] = this.props.sashDim[idx];
                    sashDim[other] = thisDim[other];

                    foreach(RectTransform rtKey in this.panes)
                    {
                        if(lastRt != null)
                        {
                            UnityEngine.UI.Image sash = sashes[new SashKey(lastRt, rtKey)];

                            Vector2 sashPos = Vector2.zero;
                            sashPos[idx] = sign * incr;

                            sash.rectTransform.anchoredPosition = sashPos;
                            sash.rectTransform.sizeDelta = sashDim;

                            incr += this.props.sashDim[idx];
                        }
                        Vector3 panePos = Vector3.zero;
                        panePos[idx] = sign * incr;
                        rtKey.anchoredPosition = panePos;

                        float paneSigSz = this.rectSizes[rtKey];
                        Vector2 paneDim = Vector3.zero;
                        paneDim[idx] = paneSigSz;
                        paneDim[other] = thisDim[other];
                        rtKey.sizeDelta = paneDim;

                        lastRt = rtKey;
                        incr += paneSigSz;
                    }
                }
            }

            public void OnRectTransformDimensionsChange()
            { 
                this.UpdateAlignment();
            }

            public void UpdateSize(RectTransform rt)
            {
                if (rectSizes.ContainsKey(rt) == false)
                    return;

                rectSizes[rt] = rt.rect.size[this.GetVectorIndex()];
            }
        }
    }
}