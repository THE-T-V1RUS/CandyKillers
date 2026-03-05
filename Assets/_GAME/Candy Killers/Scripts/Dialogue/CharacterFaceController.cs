using System.Collections;
using UnityEditor;
using UnityEngine;

public class CharacterFaceController : MonoBehaviour
{
    [SerializeField] BonesReference bonesRef;

    [SerializeField] Vector3 upperLidLeft_V3_default, lowerLidLeft_V3_default, upperLidRight_V3_default, lowerLidRight_V3_default;
    [SerializeField] Vector3 upperLidLeft_V3_blink, lowerLidLeft_V3_blink, upperLidRight_V3_blink, lowerLidRight_V3_blink;

    [System.Serializable]
    public struct FacialExpression
    {
        public bool useLids;
        public bool useLips;

        public Vector3
            browInnerLeft_V3,
            browOuterLeft_V3,
            browInnerRight_V3,
            browOuterRight_V3,
            LipLowerLeft_V3,
            LipLowerRight_V3,
            LipUpperLeft_V3,
            LipUpperRight_V3,
            upperLidLeft_V3,
            lowerLidLeft_V3,
            upperLidRight_V3,
            lowerLidRight_V3
            ;
    }

    public FacialExpression expression_default, expression_angry, expression_fear, expression_pain; 

    public enum Expression
    {
        Neutral,
        Angry,
        Fear,
        Pain,
    }

    public Expression currentExpression;

    private void Start()
    {
        StartCoroutine(blink());
        ChangeExpression(currentExpression);
    }

    IEnumerator blink(bool startInstantly = false)
    {
        if(!startInstantly)
            yield return new WaitForSeconds(Random.Range(3f, 4f));

        float blinkSpeed = Random.Range(0.1f, 0.4f);
        float halfBlink = blinkSpeed / 2f;

        // Close eyes
        yield return StartCoroutine(LerpLids(
            bonesRef.upperLidLeft.localPosition, upperLidLeft_V3_blink,
            bonesRef.lowerLidLeft.localPosition, lowerLidLeft_V3_blink,
            bonesRef.upperLidRight.localPosition, upperLidRight_V3_blink,
            bonesRef.lowerLidRight.localPosition, lowerLidRight_V3_blink,
            halfBlink
        ));

        // Open eyes
        yield return StartCoroutine(LerpLids(
            bonesRef.upperLidLeft.localPosition, upperLidLeft_V3_default,
            bonesRef.lowerLidLeft.localPosition, lowerLidLeft_V3_default,
            bonesRef.upperLidRight.localPosition, upperLidRight_V3_default,
            bonesRef.lowerLidRight.localPosition, lowerLidRight_V3_default,
            halfBlink
        ));

        StartCoroutine(blink());
    }

    IEnumerator LerpLids(
        Vector3 ulStart, Vector3 ulEnd,
        Vector3 llStart, Vector3 llEnd,
        Vector3 urStart, Vector3 urEnd,
        Vector3 lrStart, Vector3 lrEnd,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            bonesRef.upperLidLeft.localPosition = Vector3.Lerp(ulStart, ulEnd, t);
            bonesRef.lowerLidLeft.localPosition = Vector3.Lerp(llStart, llEnd, t);
            bonesRef.upperLidRight.localPosition = Vector3.Lerp(urStart, urEnd, t);
            bonesRef.lowerLidRight.localPosition = Vector3.Lerp(lrStart, lrEnd, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        bonesRef.upperLidLeft.localPosition = ulEnd;
        bonesRef.lowerLidLeft.localPosition = llEnd;
        bonesRef.upperLidRight.localPosition = urEnd;
        bonesRef.lowerLidRight.localPosition = lrEnd;
    }

    public void ChangeExpression(Expression newExpression)
    {
        StopAllCoroutines();
        currentExpression = newExpression;
        FacialExpression newFacialExpression = expression_default;

        switch (newExpression)
        {
            default:
            case Expression.Neutral:
                newFacialExpression = expression_default;
                break;

            case Expression.Angry:
                newFacialExpression = expression_angry;
                break;

            case Expression.Fear:
                newFacialExpression = expression_fear;
                break;

            case Expression.Pain:
                newFacialExpression = expression_pain;
                break;
        }

        StartCoroutine(LerpExpression(newFacialExpression,  0.25f));

        if (!newFacialExpression.useLids)
            StartCoroutine(blink(true));
    }

    IEnumerator LerpExpression(FacialExpression expression, float duration)
    {
        float elapsed = 0f;

        //Brow
        Vector3 brow_IL_Start = bonesRef.browInnerLeft.localPosition;
        Vector3 brow_OL_Start = bonesRef.browOuterLeft.localPosition;
        Vector3 brow_IR_Start = bonesRef.browInnerRight.localPosition;
        Vector3 brow_OR_Start = bonesRef.browOuterRight.localPosition;

        //Lips
        Vector3 lip_LL_Start = bonesRef.LipLowerLeft.localPosition;
        Vector3 lip_LR_Start = bonesRef.LipLowerRight.localPosition;
        Vector3 lip_UL_Start = bonesRef.LipUpperLeft.localPosition;
        Vector3 lip_UR_Start = bonesRef.LipUpperRight.localPosition;

        //Lids
        Vector3 lid_UL_Start = bonesRef.upperLidLeft.localPosition;
        Vector3 lid_LL_Start = bonesRef.lowerLidLeft.localPosition;
        Vector3 lid_UR_Start = bonesRef.upperLidRight.localPosition;
        Vector3 lid_LR_Start = bonesRef.lowerLidRight.localPosition;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            bonesRef.browInnerLeft.localPosition = Vector3.Lerp(brow_IL_Start, expression.browInnerLeft_V3, t);
            bonesRef.browOuterLeft.localPosition = Vector3.Lerp(brow_OL_Start, expression.browOuterLeft_V3, t);
            bonesRef.browInnerRight.localPosition = Vector3.Lerp(brow_IR_Start, expression.browInnerRight_V3, t);
            bonesRef.browOuterRight.localPosition = Vector3.Lerp(brow_OR_Start, expression.browOuterRight_V3, t);

            if (expression.useLids)
            {
                bonesRef.upperLidLeft.localPosition = Vector3.Lerp(lid_UL_Start, expression.upperLidLeft_V3, t);
                bonesRef.upperLidRight.localPosition = Vector3.Lerp(lid_UR_Start, expression.upperLidRight_V3, t);
                bonesRef.lowerLidRight.localPosition = Vector3.Lerp(lid_LR_Start, expression.lowerLidRight_V3, t);
                bonesRef.lowerLidLeft.localPosition = Vector3.Lerp(lid_LL_Start, expression.lowerLidLeft_V3, t);
            }

            if (expression.useLips)
            {
                bonesRef.LipLowerLeft.localPosition = Vector3.Lerp(lip_LL_Start, expression.LipLowerLeft_V3, t);
                bonesRef.LipUpperLeft.localPosition = Vector3.Lerp(lip_UL_Start, expression.LipUpperLeft_V3, t);
                bonesRef.LipLowerRight.localPosition = Vector3.Lerp(lip_LR_Start, expression.LipLowerRight_V3, t);
                bonesRef.LipUpperRight.localPosition = Vector3.Lerp(lip_UR_Start, expression.LipUpperRight_V3, t);
            }
            else
            {
                bonesRef.LipLowerLeft.localPosition = Vector3.Lerp(lip_LL_Start, expression_default.LipLowerLeft_V3, t);
                bonesRef.LipUpperLeft.localPosition = Vector3.Lerp(lip_UL_Start, expression_default.LipUpperLeft_V3, t);
                bonesRef.LipLowerRight.localPosition = Vector3.Lerp(lip_LR_Start, expression_default.LipLowerRight_V3, t);
                bonesRef.LipUpperRight.localPosition = Vector3.Lerp(lip_UR_Start, expression_default.LipUpperRight_V3, t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        bonesRef.browInnerLeft.localPosition = expression.browInnerLeft_V3;
        bonesRef.browOuterLeft.localPosition = expression.browOuterLeft_V3;
        bonesRef.browInnerRight.localPosition = expression.browInnerRight_V3;
        bonesRef.browOuterRight.localPosition = expression.browOuterRight_V3;

        if (expression.useLids)
        {
            bonesRef.upperLidLeft.localPosition = expression.upperLidLeft_V3;
            bonesRef.upperLidRight.localPosition = expression.upperLidRight_V3;
            bonesRef.lowerLidRight.localPosition = expression.lowerLidRight_V3;
            bonesRef.lowerLidLeft.localPosition = expression.lowerLidLeft_V3;
        }

        if (expression.useLips)
        {
            bonesRef.LipLowerLeft.localPosition = expression.LipLowerLeft_V3;
            bonesRef.LipUpperLeft.localPosition = expression.LipUpperLeft_V3;
            bonesRef.LipLowerRight.localPosition = expression.LipLowerRight_V3;
            bonesRef.LipUpperRight.localPosition = expression.LipUpperRight_V3;
        }
        else
        {
            bonesRef.LipLowerLeft.localPosition = expression_default.LipLowerLeft_V3;
            bonesRef.LipUpperLeft.localPosition = expression_default.LipUpperLeft_V3;
            bonesRef.LipLowerRight.localPosition = expression_default.LipLowerRight_V3;
            bonesRef.LipUpperRight.localPosition = expression_default.LipUpperRight_V3;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CharacterFaceController))]
public class CharacterFaceControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CharacterFaceController controller = (CharacterFaceController)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Expression Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Set Neutral Expression"))
        {
            controller.ChangeExpression(CharacterFaceController.Expression.Neutral);
        }

        if (GUILayout.Button("Set Angry Expression"))
        {
            controller.ChangeExpression(CharacterFaceController.Expression.Angry);
        }

        if (GUILayout.Button("Set Fear Expression"))
        {
            controller.ChangeExpression(CharacterFaceController.Expression.Fear);
        }

        if (GUILayout.Button("Set Pain Expression"))
        {
            controller.ChangeExpression(CharacterFaceController.Expression.Pain);
        }
    }
}
#endif