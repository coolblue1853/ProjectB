using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
using BehaviorDesigner.Runtime;
public class EnemyChase : EnemyAction
{
    BehaviorTree bt;
    GameObject player;
    public float moveDuration;
    public float moveDistance;
    float direction;


    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
        bt.sequence.Kill();
        isEnd = false;
        player = GameObject.FindWithTag("Player");
        StartChase();
    }
    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    public void StartChase()
    {

        bt.sequence = DOTween.Sequence()
       .AppendCallback(() => direction = Mathf.Sign(player.transform.position.x - enemyObject.transform.position.x))
        .AppendCallback(() => ChangeFace())
       .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + moveDistance * direction, moveDuration).SetEase(Ease.Linear))
       .OnComplete(() => OnSequenceComplete());

    }
    void ChangeFace()
    {
        if (direction > 0)
        {
            enemyObject.transform.localScale = new Vector3(chInRommSize, enemyObject.transform.localScale.y, 1);
        }
        else if (direction < 0)
        {
            enemyObject.transform.localScale = new Vector3(-chInRommSize, enemyObject.transform.localScale.y, 1);
        }
    }
    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {

            isEnd = true;
        }
    }

}