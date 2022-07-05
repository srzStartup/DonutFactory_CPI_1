using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;

public class LockSpriteController : MonoBehaviour
{
    public UnityAction UnlockPricePaidEvent;
    bool unlockPricePaidEventRaisedOnce = false;

    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private SpriteRenderer filler;
    [SerializeField] private SpriteRenderer filled;
    [SerializeField] private SpriteRenderer lockImage;

    string propertyName = "_Arc1";
    [SerializeField] private float _minValue = 0f;
    [SerializeField] private float _maxValue = 360f;

    Vector3 initScale;

    //int rate => GameManager.Instance.currentMoney > 10 ? 10 : 1;

    public ISetupController setupController;

    public float minValue => _minValue;
    public float maxValue => _maxValue;

    public float currentValue { get; private set; }
    public float filledRatio => currentValue / _maxValue;
    public float filledPercent => filledRatio * 100f;

    public bool deactivateOnFullyFilled { get; set; } = true;

    float cooldown = .000001f;
    float elapsedTime;

    int remainToUnlock;
    int unlockPrice;

    void Start()
    {
        setupController = GetComponentInParent<ISetupController>();
        unlockPrice = setupController.Settings.unlockPrice;
        remainToUnlock = setupController.remainToUnlock;
        textMesh.text = "$ " + remainToUnlock.ToString();

        SetFillValue(_maxValue / unlockPrice * (unlockPrice - remainToUnlock));
        initScale = transform.localScale;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            transform.localScale *= 1.2f;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player) && player.state == PlayerState.Idle)
        {
            if (filledRatio != 1)
            {
                Utils.WithCooldownPassOneParam(cooldown, ref elapsedTime, player, MakePlayerSpendMoney);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            transform.localScale = initScale;
        }
    }

    void MakePlayerSpendMoney(PlayerController player)
    {
        if (GameManager.Instance.currentMoney > 0)
        {
            if (GameManager.Instance.currentMoney >= 10)
            {
                Vector3 moneySpawnPos = player.transform.position;
                moneySpawnPos.y += 1f;
                GameObject moneyGO = ObjectPooler.Instance.SpawnFromPool("money", moneySpawnPos, Quaternion.identity, false, true);

                moneyGO.transform.DOJump(transform.position, 2f, 1, .15f)
                    .OnStart(() =>
                    {
                        moneyGO.SetActive(true);

                        GameManager.Instance.UpdateMoney(-10);
                        remainToUnlock -= 10;

                        Taptic.Light();

                        setupController.remainToUnlock = remainToUnlock;
                        ChangeBy((_maxValue / unlockPrice) * 10);
                        JSONDataManager.Instance.data.setups.Find(setup => setup.id == setupController.id).remainToUnlock = remainToUnlock;
                        JSONDataManager.Instance.SaveData();

                        if (filledRatio == 1 && !unlockPricePaidEventRaisedOnce)
                        {
                            unlockPricePaidEventRaisedOnce = true;
                            UnlockPricePaidEvent?.Invoke();
                            player.transform.DOJump(transform.position + new Vector3(7.5f, 0, 7.5f), 5, 1, .5f)
                                .OnStart(() => player.canMove = false)
                                .OnComplete(() => player.canMove = true);
                        }
                    })
                    .OnComplete(() =>
                    {
                        ObjectPooler.Instance.PushToQueue("money", moneyGO);

                        textMesh.text = "$ " + remainToUnlock.ToString();
                    });
            }
            else
            {
                Vector3 moneySpawnPos = player.transform.position;
                moneySpawnPos.y += 1f;
                GameObject moneyGO = ObjectPooler.Instance.SpawnFromPool("money", moneySpawnPos, Quaternion.identity, false, true);

                moneyGO.transform.DOJump(transform.position, 2f, 1, .15f)
                    .OnStart(() =>
                    {
                        moneyGO.SetActive(true);

                        GameManager.Instance.UpdateMoney(-1);
                        remainToUnlock -= 1;

                        Taptic.Light();

                        setupController.remainToUnlock = remainToUnlock;
                        ChangeBy((_maxValue / unlockPrice) * 1);
                        JSONDataManager.Instance.data.setups.Find(setup => setup.id == setupController.id).remainToUnlock = remainToUnlock;
                        JSONDataManager.Instance.SaveData();

                        if (filledRatio == 1 && !unlockPricePaidEventRaisedOnce)
                        {
                            unlockPricePaidEventRaisedOnce = true;
                            UnlockPricePaidEvent?.Invoke();
                        }
                    })
                    .OnComplete(() =>
                    {
                        ObjectPooler.Instance.PushToQueue("money", moneyGO);

                        textMesh.text = "$ " + remainToUnlock.ToString();
                    });
            }
        }
    }

    public void SetFillValue(float value)
    {
        if (value < _minValue)
        {
            value = _minValue;
        }
        else if (value > _maxValue)
        {
            value = _maxValue;
        }

        filler.material.SetFloat(propertyName, value);
        currentValue = value;

        if (filledRatio == 1f && deactivateOnFullyFilled)
        {
            gameObject.SetActive(false);
        }
    }

    public void ChangeBy(float changer)
    {
        currentValue += changer;

        if (currentValue + changer < _minValue)
        {
            currentValue = _minValue;
        }
        if (currentValue + changer > _maxValue)
        {
            currentValue = _maxValue;
        }

        filler.material.SetFloat(propertyName, currentValue);
    }

    public void ResetMaterial()
    {
        SetFillValue(_minValue);
    }
}
