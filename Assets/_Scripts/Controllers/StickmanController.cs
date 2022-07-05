//using System.Collections;
//using System.Collections.Generic;

//using TMPro;
//using DG.Tweening;
//using UnityEngine;
//using UnityEngine.AI;

//[DefaultExecutionOrder(-5)]
//public class StickmanController : MonoBehaviour
//{
//    [SerializeField] private Transform stickmanParent;
//    [SerializeField] private Transform moneyHolder;

//    private NavMeshAgent _agent;
//    private Animator _animator;

//    public HomeBaseController homeBase { get; private set; }
//    public StickmanState state { get; private set; }
//    public StickmanTaskState taskState { get; private set; }
//    public int moneyCapacity { get; private set; }
//    public List<Money> moniesCollected { get; private set; }
//    public Money currentMoneyTarget;
//    public NavMeshAgent agent => _agent;

//    int freeHandsVelocity;
//    int withBoxVelocity;

//    string currentAnimTrigger;

//    private void Start()
//    {
//        stickmanParent = stickmanParent == null ? transform.parent : stickmanParent;

//        _agent = stickmanParent.GetComponent<NavMeshAgent>();
//        _animator = GetComponent<Animator>();

//        this.ChangeState(StickmanState.Idle);
//        moneyCapacity = 1;
//        moniesCollected = new List<Money>();

//        freeHandsVelocity = Animator.StringToHash("_moveWithFreeHands");
//        withBoxVelocity = Animator.StringToHash("_moveWithBox");
//    }

//    private void Update()
//    {
//        if (_agent.remainingDistance > _agent.stoppingDistance)
//        {
//            this.ChangeState(moniesCollected.Count == 0 ? StickmanState.Run : StickmanState.RunWithMoney);
//        }
//        else
//        {
//            if (taskState == StickmanTaskState.Returning)
//            {
//                this.DropMoniesOff();
//            }
//            else if (taskState == StickmanTaskState.WaitingForBeingOnTheWay)
//            {
//                this.ChangeState(StickmanState.Idle);
//            }
//        }
//    }

//    public void SetHomeBase(HomeBaseController homeBase)
//    {
//        //stickmanParent.parent = homeBase.transform;
//        this.homeBase = homeBase;
//    }

//    public void CollectMoney(Money money)
//    {
//        if (moneyCapacity > moniesCollected.Count)
//        {
//            money.transform.parent = moneyHolder;

//            money.transform.DOLocalMoveY(2f, .1f)
//                .OnStart(() => 
//                {
//                    this.WaitBeforeMove(transform.position);

//                    Vector3 localRotation = money.transform.localRotation.eulerAngles;
//                    localRotation.y += 720f;
//                    localRotation.z -= 90f;
//                    money.transform.DOLocalRotate(localRotation, .5f, RotateMode.FastBeyond360)
//                        .OnComplete(() =>
//                        {
//                            money.transform.localRotation = Quaternion.Euler(.0f, .0f, -90f);
//                        });
//                })
//                .OnComplete(() =>
//                {
//                    money.transform.DOShakePosition(.5f, strength: .25f, vibrato: 20)
//                        .OnStart(() => money.Switch())
//                        .OnComplete(() =>
//                        {
//                            money.transform.DOLocalMove(Vector3.zero, .1f)
//                                .OnComplete(() =>
//                                {
//                                    moniesCollected.Add(money);

//                                    if (state != StickmanState.RunWithMoney)
//                                        this.ChangeState(StickmanState.RunWithMoney);

//                                    if (moneyCapacity == moniesCollected.Count)
//                                    {
//                                        this.ReturnBase();
//                                    }

//                                    _agent.speed -= 2f;

//                                    GameObject smokeParticleParentGO = ObjectPooler.Instance.SpawnFromPool("smoke", money.transform.position, Quaternion.identity);

//                                    if (smokeParticleParentGO)
//                                    {
//                                        Transform smokeParticleParent = smokeParticleParentGO.transform;

//                                        money.SetParticleParent(smokeParticleParent);
//                                        Vector3 localPos = Vector3.zero;
//                                        localPos.x = 1.5f;
//                                        localPos.z = 1.14f;
//                                        smokeParticleParent.localPosition = localPos;
//                                        smokeParticleParent.localScale = Vector3.one;
//                                        smokeParticleParent.localRotation = Quaternion.Euler(0f, 0f, 90f);

//                                        foreach (ParticleSystem smokeParticle in smokeParticleParent.GetComponentsInChildren<ParticleSystem>())
//                                        {
//                                            smokeParticle.Play();
//                                        }
//                                    }
//                                });
//                        });
//                });
//        }
//    }

//    public void DropMoniesOff()
//    {
//        foreach (Money money in moniesCollected)
//        {
//            money.transform.parent = null;
//           // money.gameObject.SetActive(false);

//            GameManager.Instance.UpdateMoney(money.worth);

//            Transform particleParent = money.DetachParticleParent();

//            foreach (ParticleSystem smokeParticle in particleParent.GetComponentsInChildren<ParticleSystem>())
//            {
//                smokeParticle.Stop();
//            }

//            ObjectPooler.Instance.PushToQueue("smoke", particleParent.gameObject);

//            money.ResetAll();

//            ObjectPooler.Instance.PushToQueue("money", money.gameObject);

//            Transform moneySpriteTransform = ObjectPooler.Instance.SpawnFromPool("moneySprite", stickmanParent.position, Quaternion.Euler(45f, 0f, 0f), instantEnqueue: true).transform;
//            moneySpriteTransform.localScale = new Vector3(.5f, .5f, .5f);
//            moneySpriteTransform.gameObject.SetActive(true);
//            TextMeshPro textMesh = moneySpriteTransform.Find("AmountText").GetComponent<TextMeshPro>();
//            textMesh.text = "+" + money.worth;
//            moneySpriteTransform.DOMoveY(moneySpriteTransform.position.y + 5f, .5f)
//                .OnComplete(() => moneySpriteTransform.gameObject.SetActive(false));
//        }

//        moniesCollected.Clear();
//        currentMoneyTarget = null;
//        _agent.speed += 2f;

//        this.ChangeTaskState(StickmanTaskState.Available);
//    }

//    public void ReturnBase()
//    {
//        Vector3 dest = homeBase.transform.position;
//        dest.y = stickmanParent.position.y;

//        _agent.SetDestination(dest);
//        this.ChangeTaskState(StickmanTaskState.Returning);
//    }

//    public void SetTargetMoney(Money target)
//    {
//        target.isTargeted = true;
//        currentMoneyTarget = target;
//    }

//    public void SetTargetField(FieldController field)
//    {
//        _agent.SetDestination(field.transform.position);
//        this.ChangeTaskState(StickmanTaskState.OnTheWay);
//    }

//    public void MoveTo(Vector3 position)
//    {
//        _agent.SetDestination(position);
//        this.ChangeState(StickmanState.Run);
//    }

//    public void WaitBeforeMove(Vector3 positionToMove)
//    {
//        this.MoveTo(positionToMove);
//        this.ChangeTaskState(StickmanTaskState.WaitingForBeingOnTheWay);
//    }

//    public void ChangeState(StickmanState state, bool force = false)
//    {
//        if (state == this.state && !force)
//            return;

//        this.state = state;

//        HandleAnimator();
//    }

//    public void ChangeTaskState(StickmanTaskState taskState)
//    {
//        if (taskState == this.taskState)
//            return;

//        this.taskState = taskState;
//    }

//    private void HandleAnimator()
//    {
//        switch (this.state)
//        {
//            case StickmanState.Idle:

//                //_animator.SetFloat(freeHandsVelocity, 0);
//                //_animator.SetBool("_isCarryingBox", false);
//                TriggerAnim("_idle");
//                break;

//            case StickmanState.RunWithMoney:
//                TriggerAnim("_carry");
//                //_animator.SetBool("_isCarryingBox", true);
//                //_animator.SetFloat(withBoxVelocity, 1);
//                break;
//            default:
//                //_animator.SetFloat(freeHandsVelocity, 10);
//                //_animator.SetBool("_isCarryingBox", false);
//                TriggerAnim("_run");

//                _animator.SetFloat(freeHandsVelocity, 10);
//                break;
//        }
//    }

//    private void TriggerAnim(string animTrigger)
//    {
//        if (currentAnimTrigger != null)
//        {
//            _animator.ResetTrigger(currentAnimTrigger);
//        }

//        _animator.SetTrigger(animTrigger);
//        currentAnimTrigger = animTrigger;
//    }
//}

//public enum StickmanState
//{
//    Idle,
//    Run,
//    RunWithMoney
//}

//public enum StickmanTaskState
//{
//    Spawned,
//    Available,
//    OnTheWay,
//    WaitingForBeingOnTheWay,
//    WaitingForMoneySpawn,
//    Returning
//}
