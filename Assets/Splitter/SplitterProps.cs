using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIUtils
    {
        [CreateAssetMenu(menuName = "PxPre/SplitterProps")]
        public class SplitterProps : ScriptableObject
        {
            public Sprite spriteHoriz; // Only applies to things split horizontally
            public Sprite spriteVert;  // Only applies to things split vertically

   
            public Vector2 sashDim = new Vector2(10.0f, 10.0f);
            public Color sashColor = Color.white;
        }
    }
}