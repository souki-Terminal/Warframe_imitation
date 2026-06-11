using UnityEngine;

public class ResetSlash : StateMachineBehaviour
{
    // アニメーションの再生が完全に終了し、別のステートへ移行する時に呼ばれる処理
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // slashのチェックを外す（falseにする）
        animator.SetBool("slash", false);
    }
}