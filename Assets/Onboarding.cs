using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(10)]
public class Onboarding : MonoBehaviour
{
    [SerializeField] private GameObject[] toggleGameObjects;
    [SerializeField] private TargetObject[] toggleTargetObjects;

    [SerializeField] private Transform onboardingArrow;
    [SerializeField] private FloatingJoystick joystick;

    [Header("PasteProvider")]
    [SerializeField] private Transform pasteProviderRefPoint;
    [SerializeField] private Transform pasteProviderTransform;
    [SerializeField] private TriggerIndicatorAnim pasteProviderIndicatorAnim;

    [Header("DonutRawPreparer")]
    [SerializeField] private Transform donutRawPreparerRefPoint_Provide;
    [SerializeField] private Transform donutRawPreparerRefPoint_Consume;
    [SerializeField] private Transform donutRawPreparerTransform_Provide;
    [SerializeField] private Transform donutRawPreparerTransform_Consume;
    [SerializeField] private TriggerIndicatorAnim donutRawPreparerIndicatorAnim_Provide;
    [SerializeField] private TriggerIndicatorAnim donutRawPreparerIndicatorAnim_Consume;

    [Header("Oven")]
    [SerializeField] private Transform ovenRefPoint;
    [SerializeField] private Transform ovenTransform;
    [SerializeField] private TriggerIndicatorAnim ovenIndicatorAnim;

    [Header("SauceSpiller")]
    [SerializeField] private Transform sauceSpillerRefPoint_Provide;
    [SerializeField] private Transform sauceSpillerRefPoint_Consume;
    [SerializeField] private Transform sauceSpillerTransform_Provide;
    [SerializeField] private Transform sauceSpillerTransform_Consume;
    [SerializeField] private TriggerIndicatorAnim sauceSpillerIndicatorAnim_Provide;
    [SerializeField] private TriggerIndicatorAnim sauceSpillerIndicatorAnim_Consume;

    [Header("Showroom")]
    [SerializeField] private Transform showroomRefPoint;
    [SerializeField] private Transform showroomRefPoint_CollectMoney;
    [SerializeField] private Transform showroomTransform;
    [SerializeField] private TriggerIndicatorAnim showroomIndicatorAnim;

    [SerializeField] private InGameEventChannel eventChannel;

    bool questZeroAssigned = false;

    bool pasteCollectedOnce = false;
    bool panWithRawDonutsCollectedOnce = false;
    bool panWithBakedDontsCollectedOnce = false;
    bool donutsSaucedCollectedOnce = false;

    bool pasteConsumedByDonutRawPreparerOnce = false;
    bool panWithRawDonutsConsumedByOvenOnce = false;

    bool saucedDonutReadyEventRaisedOnce = false;
    bool saucedDonutConsumedByShowroomOnce = false;

    bool moneyUpdatedOnce = false;

    TutorialQuest currentQuest;

    PlayerController player => PlayerController.Instance;
    CameraController camController => CameraController.Instance;

    void Awake()
    {
        if (JSONDataManager.Instance.data.onboardingDone)
        {
            Destroy(gameObject);
        }

        eventChannel.GameStartedEvent += OnGameStarted;

        eventChannel.PasteConsumeByDonutRawPreparerEvent += OnPasteConsumeByDonutRawPreparer;
        eventChannel.PanWithRawDonutsReadyEvent += OnPanWithRawDonutsReady;
        eventChannel.PanWithRawDonutsConsumeByOvenEvent += OnPanWithRawDonutsConsumeByOven;
        eventChannel.PanWithBakedDonutsReadyEvent += OnPanWithBakedDonutsReadyEvent;
        eventChannel.PanWithBakedDonutsConsumeBySauceSpillerEvent += OnPanWithBakedDonutsConsumeBySauceSpiller;
        eventChannel.SaucedDonutReadyEvent += OnSaucedDonutReady;
        eventChannel.SaucedDonutConsumeByShowroomEvent += OnSaucedDonutConsumeByShowroom;
        eventChannel.MoneyUpdatedEvent += OnMoneyUpdated;
    }

    void OnDestroy()
    {
        eventChannel.GameStartedEvent -= OnGameStarted;
        eventChannel.PasteConsumeByDonutRawPreparerEvent -= OnPasteConsumeByDonutRawPreparer;
        eventChannel.PanWithRawDonutsReadyEvent -= OnPanWithRawDonutsReady;
        eventChannel.PanWithRawDonutsConsumeByOvenEvent -= OnPanWithRawDonutsConsumeByOven;
        eventChannel.PanWithBakedDonutsReadyEvent -= OnPanWithBakedDonutsReadyEvent;
        eventChannel.PanWithBakedDonutsConsumeBySauceSpillerEvent -= OnPanWithBakedDonutsConsumeBySauceSpiller;
        eventChannel.SaucedDonutReadyEvent -= OnSaucedDonutReady;
        eventChannel.SaucedDonutConsumeByShowroomEvent -= OnSaucedDonutConsumeByShowroom;
        eventChannel.MoneyUpdatedEvent -= OnMoneyUpdated;
    }

    void Start()
    {
        foreach (GameObject go in toggleGameObjects)
        {
            go.SetActive(false);
        }

        foreach (TargetObject to in toggleTargetObjects)
        {
            to.isActive = false;
        }
    }

    void Update()
    {
        if (currentQuest == TutorialQuest.QuestZero && !questZeroAssigned)
        {
            questZeroAssigned = true;
            camController.LockAndFocus(pasteProviderTransform.position,
                onStart: (sequence) => 
                {
                    UIManager.Instance.CloseUpgradePanel();
                    joystick.gameObject.SetActive(false);
                    player.canMove = false;
                    pasteProviderIndicatorAnim.gameObject.SetActive(true);
                    pasteProviderIndicatorAnim.AnimateTextBox();
                    onboardingArrow.position = pasteProviderRefPoint.position;
                    onboardingArrow.gameObject.SetActive(true);
                    Taptic.Light();
                },
                onComplete: (sequence) => 
                {
                    camController.BackToTarget(
                        delay: 1f,
                        onComplete: (sequence) => 
                        {
                            currentQuest = TutorialQuest.CollectPaste;
                            joystick.gameObject.SetActive(true);
                            player.canMove = true;
                        });
                });
        }

        if (!pasteCollectedOnce && player.stackManager.Count != 0 && player.taskState == PlayerTaskState.Available)
        {
            pasteCollectedOnce = true;

            camController.LockAndFocus(donutRawPreparerTransform_Provide.position,
               onStart: (sequence) =>
               {
                   UIManager.Instance.CloseUpgradePanel();
                   joystick.gameObject.SetActive(false);
                   player.canMove = false;
                   pasteProviderIndicatorAnim.gameObject.SetActive(false);
                   pasteProviderIndicatorAnim.StopAnim();
                   donutRawPreparerIndicatorAnim_Provide.gameObject.SetActive(true);
                   donutRawPreparerIndicatorAnim_Provide.AnimateTextBox();
                   onboardingArrow.position = donutRawPreparerRefPoint_Provide.position;
                   onboardingArrow.gameObject.SetActive(true);
                   Taptic.Light();
               },
               onComplete: (sequence) =>
               {
                   camController.BackToTarget(
                       delay: 1f,
                       onComplete: (sequence) =>
                       {
                           currentQuest = TutorialQuest.ProvidePasteToPreparer;
                           joystick.gameObject.SetActive(true);
                           player.canMove = true;
                       });
               });
        }

        if (!panWithRawDonutsCollectedOnce && player.stackManager.Count != 0 && player.stackManager.First().type == CollectibleType.PanWithRawDonuts)
        {
            panWithRawDonutsCollectedOnce = true;

            camController.LockAndFocus(ovenTransform.position,
               onStart: (sequence) =>
               {
                   UIManager.Instance.CloseUpgradePanel();
                   joystick.gameObject.SetActive(false);
                   player.canMove = false;
                   donutRawPreparerIndicatorAnim_Consume.gameObject.SetActive(false);
                   donutRawPreparerIndicatorAnim_Consume.StopAnim();
                   ovenIndicatorAnim.gameObject.SetActive(true);
                   ovenIndicatorAnim.AnimateTextBox();
                   onboardingArrow.position = ovenRefPoint.position;
                   onboardingArrow.gameObject.SetActive(true);
                   Taptic.Light();
               },
               onComplete: (sequence) =>
               {
                   camController.BackToTarget(
                       delay: 1f,
                       onComplete: (sequence) =>
                       {
                           currentQuest = TutorialQuest.ProvidePanToOven;
                           joystick.gameObject.SetActive(true);
                           player.canMove = true;
                       });
               });
        }

        if (!panWithBakedDontsCollectedOnce && player.stackManager.Count != 0 && player.stackManager.First().type == CollectibleType.PanWithBakedDonuts)
        {
            panWithBakedDontsCollectedOnce = true;

            camController.LockAndFocus(sauceSpillerTransform_Provide.position,
               onStart: (sequence) =>
               {
                   UIManager.Instance.CloseUpgradePanel();
                   joystick.gameObject.SetActive(false);
                   player.canMove = false;
                   ovenIndicatorAnim.gameObject.SetActive(false);
                   ovenIndicatorAnim.StopAnim();
                   sauceSpillerIndicatorAnim_Provide.gameObject.SetActive(true);
                   sauceSpillerIndicatorAnim_Provide.AnimateTextBox();
                   onboardingArrow.position = sauceSpillerRefPoint_Provide.position;
                   onboardingArrow.gameObject.SetActive(true);
                   Taptic.Light();
               },
               onComplete: (sequence) =>
               {
                   camController.BackToTarget(
                       delay: 1f,
                       onComplete: (sequence) =>
                       {
                           currentQuest = TutorialQuest.ProvidePanToSauceSpiller;
                           joystick.gameObject.SetActive(true);
                           player.canMove = true;
                       });
               });
        }

        if (!donutsSaucedCollectedOnce && player.stackManager.Count != 0 && player.stackManager.First().type == CollectibleType.DonutSaucedChocolate)
        {
            donutsSaucedCollectedOnce = true;

            camController.LockAndFocus(showroomTransform.position,
               onStart: (sequence) =>
               {
                   UIManager.Instance.CloseUpgradePanel();
                   joystick.gameObject.SetActive(false);
                   player.canMove = false;
                   sauceSpillerIndicatorAnim_Consume.gameObject.SetActive(false);
                   sauceSpillerIndicatorAnim_Consume.StopAnim();
                   showroomIndicatorAnim.gameObject.SetActive(true);
                   showroomIndicatorAnim.AnimateTextBox();
                   onboardingArrow.position = showroomRefPoint.position;
                   onboardingArrow.gameObject.SetActive(true);
                   Taptic.Light();
               },
               onComplete: (sequence) =>
               {
                   camController.BackToTarget(
                       delay: 1f,
                       onComplete: (sequence) =>
                       {
                           currentQuest = TutorialQuest.PutDonutsToShowroom;
                           joystick.gameObject.SetActive(true);
                           player.canMove = true;
                       });
               });
        }
    }

    void OnGameStarted()
    {
        currentQuest = TutorialQuest.QuestZero;
    }

    void OnPasteConsumeByDonutRawPreparer()
    {
        if (!pasteConsumedByDonutRawPreparerOnce)
        {
            pasteConsumedByDonutRawPreparerOnce = true;

            donutRawPreparerIndicatorAnim_Provide.gameObject.SetActive(false);
            donutRawPreparerIndicatorAnim_Provide.StopAnim();
            onboardingArrow.gameObject.SetActive(false);
            currentQuest = TutorialQuest.CollectPanWithRawDonuts;
        }
    }

    void OnPanWithRawDonutsReady()
    {
        if (currentQuest == TutorialQuest.CollectPanWithRawDonuts)
        {
            camController.LockAndFocus(donutRawPreparerTransform_Consume.position,
               onStart: (sequence) =>
               {
                   UIManager.Instance.CloseUpgradePanel();
                   joystick.gameObject.SetActive(false);
                   player.canMove = false;
                   onboardingArrow.position = donutRawPreparerRefPoint_Consume.position;
                   onboardingArrow.gameObject.SetActive(true);
                   donutRawPreparerIndicatorAnim_Consume.gameObject.SetActive(true);
                   donutRawPreparerIndicatorAnim_Consume.AnimateTextBox();
                   Taptic.Light();
               },
               onComplete: (sequence) =>
               {
                   camController.BackToTarget(
                       delay: 1f,
                       onComplete: (sequence) =>
                       {
                           joystick.gameObject.SetActive(true);
                           player.canMove = true;
                       });
               });
        }
    }

    void OnPanWithRawDonutsConsumeByOven()
    {
        if (!panWithRawDonutsConsumedByOvenOnce)
        {
            panWithRawDonutsConsumedByOvenOnce = true;

            ovenIndicatorAnim.gameObject.SetActive(false);
            ovenIndicatorAnim.StopAnim();
            onboardingArrow.gameObject.SetActive(false);
            currentQuest = TutorialQuest.CollectPanWithBakedDonuts;
        }
    }

    void OnPanWithBakedDonutsReadyEvent()
    {
        if (currentQuest == TutorialQuest.CollectPanWithBakedDonuts)
        {
            camController.LockAndFocus(ovenTransform.position,
               onStart: (sequence) =>
               {
                   UIManager.Instance.CloseUpgradePanel();
                   joystick.gameObject.SetActive(false);
                   player.canMove = false;
                   onboardingArrow.position = ovenRefPoint.position;
                   onboardingArrow.gameObject.SetActive(true);
                   ovenIndicatorAnim.gameObject.SetActive(true);
                   ovenIndicatorAnim.AnimateTextBox();
                   Taptic.Light();
               },
               onComplete: (sequence) =>
               {
                   camController.BackToTarget(
                       delay: 1f,
                       onComplete: (sequence) =>
                       {
                           joystick.gameObject.SetActive(true);
                           player.canMove = true;
                       });
               });
        }
    }

    void OnPanWithBakedDonutsConsumeBySauceSpiller()
    {
        if (currentQuest == TutorialQuest.ProvidePanToSauceSpiller)
        {
            sauceSpillerIndicatorAnim_Provide.gameObject.SetActive(false);
            sauceSpillerIndicatorAnim_Provide.StopAnim();
            onboardingArrow.gameObject.SetActive(false);
            currentQuest = TutorialQuest.CollectPanWithSaucedDonuts;
        }
    }

    void OnSaucedDonutReady()
    {
        if (currentQuest == TutorialQuest.CollectPanWithSaucedDonuts && !saucedDonutReadyEventRaisedOnce)
        {
            saucedDonutReadyEventRaisedOnce = true;
            camController.LockAndFocus(sauceSpillerTransform_Consume.position,
               onStart: (sequence) =>
               {
                   UIManager.Instance.CloseUpgradePanel();
                   joystick.gameObject.SetActive(false);
                   player.canMove = false;
                   onboardingArrow.position = sauceSpillerRefPoint_Consume.position;
                   onboardingArrow.gameObject.SetActive(true);
                   sauceSpillerIndicatorAnim_Consume.gameObject.SetActive(true);
                   sauceSpillerIndicatorAnim_Consume.AnimateTextBox();
                   Taptic.Light();
               },
               onComplete: (sequence) =>
               {
                   camController.BackToTarget(
                       delay: 1f,
                       onComplete: (sequence) =>
                       {
                           joystick.gameObject.SetActive(true);
                           player.canMove = true;
                       });
               });
        }
    }

    void OnSaucedDonutConsumeByShowroom()
    {
        if (!saucedDonutConsumedByShowroomOnce)
        {
            saucedDonutConsumedByShowroomOnce = true;

            JSONDataManager.Instance.data.onboardingDone = true;
            StandManager.Instance.setups.FindAll(setup => !setup.IsUnlocked)
                .ForEach(setup =>
                {
                    if (!setup.IsUnlocked)
                    {
                        setup.LockSprite.gameObject.SetActive(true);
                    }
                });

            foreach (GameObject go in toggleGameObjects)
            {
                go.SetActive(true);
            }

            foreach (TargetObject to in toggleTargetObjects)
            {
                to.OpenUI();
            }

            showroomIndicatorAnim.gameObject.SetActive(false);
            showroomIndicatorAnim.StopAnim();
            onboardingArrow.gameObject.SetActive(false);
            onboardingArrow.position = showroomRefPoint_CollectMoney.position;
            onboardingArrow.gameObject.SetActive(true);
            currentQuest = TutorialQuest.CollectMoney;
        }
    }

    void OnMoneyUpdated(int updatedAmount)
    {
        if (!moneyUpdatedOnce)
        {
            moneyUpdatedOnce = true;
            onboardingArrow.gameObject.SetActive(false);
        }
    }
}

enum TutorialQuest
{
    QuestZero,
    CollectPaste,
    ProvidePasteToPreparer,
    CollectPanWithRawDonuts,
    ProvidePanToOven,
    CollectPanWithBakedDonuts,
    ProvidePanToSauceSpiller,
    CollectPanWithSaucedDonuts,
    PutDonutsToShowroom,
    CollectMoney
}
