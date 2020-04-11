using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{ 
    namespace UIUtils
    { 
        public class SplitterSash : 
            UnityEngine.UI.Image,
            UnityEngine.EventSystems.IDragHandler
        { 
            public Splitter parent;
            public RectTransform paneA;
            public RectTransform paneB;

            void UnityEngine.EventSystems.IDragHandler.OnDrag(
                UnityEngine.EventSystems.PointerEventData eventData)
            {
                int vidx = this.parent.GetVectorIndex();
                float delta = eventData.delta[vidx];
                if(delta == 0.0f)
                    return;

                float minsize = parent.minSize;
                
                float szA = paneA.rect.size[vidx];
                float szB = paneB.rect.size[vidx];

                // When we do the actual math, we're going to break out in
                // every single permutation of grain (horiz and vert) and 
                // direction (positive delta and negative delta).
                //
                // Because every case is being explicitly handled, we're not
                // going to worry about mechanization and indexing, and just
                // access components explicitly to save some sanity.
                //
                // The main thing stopping us from mechanizing this, is that 
                // I don't also want to throw the sign of the grain (how positive
                // Y is up, instead of the more convenient down direction) into 
                // the mix.
                if(vidx == 0)
                { 
                    if(delta < 0.0f)
                    { 
                        float endSize = Mathf.Max(minsize, szA + delta);
                        delta = endSize - szA;
                    }
                    else
                    {
                        float endSize = Mathf.Max(minsize, szB - delta);
                        delta = szB - endSize;
                    }
                    if(delta == 0.0f)
                        return;

                    paneA.sizeDelta = new Vector2(paneA.sizeDelta.x + delta, paneA.sizeDelta.y);
                    paneB.anchoredPosition = new Vector2(paneB.anchoredPosition.x + delta, paneB.anchoredPosition.y);
                    paneB.sizeDelta = new Vector2(paneB.sizeDelta.x - delta, paneB.sizeDelta.y);

                    this.rectTransform.anchoredPosition = 
                        new Vector2(
                            this.rectTransform.anchoredPosition.x + delta, 
                            this.rectTransform.anchoredPosition.y);
                }
                // Veritical handler
                else
                { 
                    if(delta < 0.0f)
                    { 
                        float endSize = Mathf.Max(minsize, szB + delta);
                        delta = endSize - szB;
                    }
                    else
                    { 
                        float endSize = Mathf.Max(minsize, szA - delta);
                        delta = szA - endSize;
                    }
                    if(delta == 0.0f)
                        return;

                    paneA.sizeDelta = new Vector2(paneA.sizeDelta.x, paneA.sizeDelta.y - delta);
                    paneB.anchoredPosition = new Vector2(paneB.anchoredPosition.x, paneB.anchoredPosition.y + delta);
                    paneB.sizeDelta = new Vector2(paneB.sizeDelta.x, paneB.sizeDelta.y + delta);

                    this.rectTransform.anchoredPosition =
                        new Vector2(
                            this.rectTransform.anchoredPosition.x,
                            this.rectTransform.anchoredPosition.y + delta);
                }

                // If we've gotten this far, something was modified
                // We need to update the individual records for the next 
                // time a full update is done.
                this.parent.UpdateSize(paneA);
                this.parent.UpdateSize(paneB);
            }
        }
    }
}