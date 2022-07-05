using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[DefaultExecutionOrder(5)]
public abstract class SetupController<T> : MonoBehaviour, ISetupController where T : ControllerSettings
{
    public Transform cachedTransform { get; set; }
    public int remainToUnlock { get; set; }
    public virtual SetupControllerType type { get; }
    public int id { get; set; }

    public GameObject[] toggleGameObjects;
    public bool isUnlocked;
    public bool IsUnlocked { get { return isUnlocked; } set { isUnlocked = value; } }
    public T settings;
    public ControllerSettings Settings => settings;
    public LockSpriteController lockSprite;
    public LockSpriteController LockSprite => lockSprite;

    protected virtual void Start()
    {
        lockSprite.UnlockPricePaidEvent += OnUnlockPricePaid;

        cachedTransform = transform;

        if (isUnlocked)
        {
            isUnlocked = true;
            enabled = true;

            lockSprite.gameObject.SetActive(false);

            foreach (GameObject go in toggleGameObjects)
            {
                go.SetActive(true);
            }

            if (GetComponent<Collider>())
            {
                GetComponent<Collider>().enabled = true;
            }
        }
        else
        {
            enabled = false;
            lockSprite.gameObject.SetActive(JSONDataManager.Instance.data.onboardingDone);

            foreach (GameObject go in toggleGameObjects)
            {
                go.SetActive(false);
            }
        }
    }

    protected abstract void OnUnlock();
    protected virtual void Unlock()
    {
        isUnlocked = true;
        enabled = true;

        lockSprite.gameObject.SetActive(false);

        foreach (GameObject go in toggleGameObjects)
        {
            go.SetActive(true);
        }

        if (GetComponent<Collider>())
        {
            GetComponent<Collider>().enabled = true;
        }

        JSONDataManager.Instance.data.setups.Find(setupData => setupData.id == id).isUnlocked = true;
        JSONDataManager.Instance.SaveData();

        ElephantSDK.Elephant.Event("Machine" + id + "_open", 1);
    }

    void OnUnlockPricePaid()
    {
        enabled = true;

        Unlock();

        OnUnlock();
    }

    void ISetupController.TriggerUpgrade()
    {
        OnUpgradeTriggerEnter();
    }

    protected virtual void OnUpgradeTriggerEnter()
    {

    }

    void ISetupController.Unlock()
    {
        Unlock();
    }
}

public enum SetupControllerType
{
    None,
    DonutRawPreparer,
    Oven,
    SauceSpiller,
    CandySpiller,
}
