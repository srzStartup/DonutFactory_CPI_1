using System.Collections;
using System.Collections.Generic;

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : Singleton<UIManager>
{
    [Header("Screens")]
    [SerializeField] private RectTransform inGameScreen;
    [SerializeField] private RectTransform startScreen;
    [SerializeField] private RectTransform winScreen;
    [SerializeField] private RectTransform loseScreen;

    [Space(5)]

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI inGameScreen_CollectibleText;
    [SerializeField] private TextMeshProUGUI inGameScreen_LevelText;
    [SerializeField] private TextMeshProUGUI startScreen_LevelText;
    [SerializeField] private TextMeshProUGUI startScreen_CollectibleText;
    [SerializeField] private TextMeshProUGUI winScreen_LevelEndMoneyText;
    [SerializeField] private TextMeshProUGUI winScreen_CollectibleText;

    [Space(5)]

    [Header("Colors")]

    [Space(5)]

    [Header("Images")]
    [SerializeField] private Transform tapToStartImage;
    [SerializeField] private List<GameObject> lights;
    [SerializeField] private RectTransform winScreen_moneyImageRef;

    [Space(5)]
    [Header("Lights")]
    [SerializeField] private RectTransform winScreenLight;

    [Space(5)]
    [Header("LevelEnd")]
    [SerializeField] private RectTransform winScreen_incomeBG;
    [SerializeField] private Button nextButton;

    [Space(5)]

    public List<TargetIndicator> targetIndicators = new List<TargetIndicator>();
    public Transform targetIndicatorsParent;
    public GameObject TargetIndicatorPrefab;

    [Space(5)]

    [Header("DonutPreparer Upgrade")]
    [SerializeField] private GameObject donutPreparerUpgradePanel;
    [SerializeField] private TextMeshProUGUI donutPreparerUpgradeTypeText;
    [SerializeField] private TextMeshProUGUI donutPreparerUpgradePriceText;
    [SerializeField] private Button donutPreparerCapacityButton;
    [SerializeField] private Image donutPreparerCapacityButtonDisabled;
    [SerializeField] private TextMeshProUGUI donutPreparerUpgradeTypeTextDisabled;
    [SerializeField] private TextMeshProUGUI donutPreparerUpgradePriceTextDisabled;
    [SerializeField] private Image donutPreparerCapacityButtonMoneyImage;

    [Header("Oven Upgrade")]
    [SerializeField] private GameObject ovenUpgradePanel;
    [SerializeField] private TextMeshProUGUI ovenSpeedUpgradeTypeText;
    [SerializeField] private TextMeshProUGUI ovenSpeedUpgradePriceText;
    [SerializeField] private TextMeshProUGUI ovenCapacityUpgradeTypeText;
    [SerializeField] private TextMeshProUGUI ovenCapacityUpgradePriceText;

    [SerializeField] private TextMeshProUGUI ovenSpeedUpgradeTypeTextDisabled;
    [SerializeField] private TextMeshProUGUI ovenSpeedUpgradePriceTextDisabled;
    [SerializeField] private TextMeshProUGUI ovenCapacityUpgradeTypeTextDisabled;
    [SerializeField] private TextMeshProUGUI ovenCapacityUpgradePriceTextDisabled;

    [SerializeField] private Button ovenCapacityButton;
    [SerializeField] private Button ovenSpeedButton;

    [SerializeField] private Image ovenCapacityButtonDisabled;
    [SerializeField] private Image ovenSpeedButtonDisabled;

    [SerializeField] private Image ovenCapacityButtonMoneyImage;
    [SerializeField] private Image ovenSpeedButtonMoneyImage;

    [Header("Player Upgrade")]
    [SerializeField] private GameObject playerUpgradePanel;
    [SerializeField] private TextMeshProUGUI playerSpeedUpgradeTypeText;
    [SerializeField] private TextMeshProUGUI playerSpeedUpgradePriceText;
    [SerializeField] private TextMeshProUGUI playerCapacityUpgradeTypeText;
    [SerializeField] private TextMeshProUGUI playerCapacityUpgradePriceText;

    [SerializeField] private TextMeshProUGUI playerSpeedUpgradeTypeTextDisabled;
    [SerializeField] private TextMeshProUGUI playerSpeedUpgradePriceTextDisabled;
    [SerializeField] private TextMeshProUGUI playerCapacityUpgradeTypeTextDisabled;
    [SerializeField] private TextMeshProUGUI playerCapacityUpgradePriceTextDisabled;

    [SerializeField] private Button playerSpeedUpgradeButton;
    [SerializeField] private Button playerCapacityUpgradeButton;

    [SerializeField] private Image playerSpeedUpgradeButtonDisabled;
    [SerializeField] private Image playerCapacityUpgradeButtonDisabled;

    [SerializeField] private Image playerSpeedButtonMoneyImage;
    [SerializeField] private Image playerCapacityButtonMoneyImage;

    [Space(5)]

    [Header("Event Channels")]
    [SerializeField] private InGameEventChannel inGameEventChannel;

    GameObject currentPanel;
    ISetupController currentSetup;
    SetupControllerType currentSetupType;

    protected override void Awake()
    {
        base.Awake();

        inGameEventChannel.GameStartedEvent += OnGameStarted;
        inGameEventChannel.LevelStartedEvent += OnLevelStarted;
        inGameEventChannel.LevelAccomplishedEvent += OnLevelAccomplished;
        inGameEventChannel.MoneyUpdatedEvent += OnMoneyUpdated;
    }

    void OnDestroy()
    {
        inGameEventChannel.GameStartedEvent -= OnGameStarted;
        inGameEventChannel.LevelStartedEvent -= OnLevelStarted;
        inGameEventChannel.LevelAccomplishedEvent -= OnLevelAccomplished;
        inGameEventChannel.MoneyUpdatedEvent -= OnMoneyUpdated;
    }

    private void Start()
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        //StartCoroutine(BoostsAnimationCoroutine(/*buraya boostlar gelecek*/));
    }

    private void Update()
    {
        if (targetIndicators.Count > 0)
        {
            for (int i = 0; i < targetIndicators.Count; i++)
            {
                targetIndicators[i].UpdateTargetIndicator();
            }
        }
    }

    private void OnGameStarted()
    {
        //ElephantSDK.Elephant.LevelStarted(currentLevel);

        //startScreen_LevelText.text = "Level " + GameManager.Instance.level.ToString();
        //startScreen_CollectibleText.text = Mathf.RoundToInt(GameManager.Instance.currentMoney).ToString();
        inGameScreen_CollectibleText.text = Mathf.RoundToInt(GameManager.Instance.currentMoney).ToString();
        inGameScreen_LevelText.text = "Level " + GameManager.Instance.level.ToString();
    }


    private void OnLevelStarted()
    {
        //inGameScreen_CollectibleText.text = Mathf.RoundToInt(GameManager.Instance.currentMoney).ToString();
        //inGameScreen_LevelText.text = "Level " + GameManager.Instance.level.ToString();

        //startScreen.gameObject.SetActive(false);
        //inGameScreen.gameObject.SetActive(true);
    }

    private void OnLevelAccomplished()
    {
        winScreen_CollectibleText.text = Mathf.RoundToInt(GameManager.Instance.currentMoney).ToString();
        winScreenLight.DORotate(new Vector3(0, 0, 360), 10f, RotateMode.FastBeyond360)
            .SetLoops(-1)
            .SetEase(Ease.Linear);

        inGameScreen.gameObject.SetActive(false);
        winScreen.gameObject.SetActive(true);
    }

    private void OnMoneyUpdated(int updatedAmount)
    {
        if (updatedAmount > 0)
        {
            inGameScreen_CollectibleText.rectTransform.DORewind();
            inGameScreen_CollectibleText.rectTransform
                .DOPunchScale(inGameScreen_CollectibleText.rectTransform.localScale * 1.25f, .25f)
                .OnStart(() =>
                {
                    //inGameScreen_CollectibleText.text = Mathf.RoundToInt(GameManager.Instance.currentMoney).ToString();
                    inGameScreen_CollectibleText.text = GameManager.Instance.currentMoney.ToString();
                });
        }
        else
        {
            //inGameScreen_CollectibleText.text = Mathf.RoundToInt(GameManager.Instance.currentMoney).ToString();
            inGameScreen_CollectibleText.text = GameManager.Instance.currentMoney.ToString();
        }
    }

    IEnumerator BoostsAnimationCoroutine(params Transform[] boosts)
    {
        yield return new WaitForSeconds(2.0f);

        Sequence sequence = DOTween.Sequence();

        Ease ease = Ease.InOutBounce;
        Vector3 punchScale = Vector3.one * .1f;
        float duration = .5f;
        int vibrato = 1;


        foreach (Transform boost in boosts)
        {
            sequence.Join(boost.DOPunchScale(punchScale, duration, vibrato).SetEase(ease));
        }

        sequence.SetLoops(-1, LoopType.Yoyo);
        sequence.SetDelay(1.5f);
    }

    IEnumerator UpdateMoneyDelayed(int counter, float delay)
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < counter; i++)
        {
            GameManager.Instance.UpdateMoney(1);
            yield return new WaitForSeconds(delay);
        }
    }

    #region Button Clicks

    public void TapToStartButtonClick()
    {
        //Taptic.Light();

        //startScreen.gameObject.SetActive(false);
        //inGameScreen.gameObject.SetActive(true);

        //GameManager.Instance.StartLevel();
    }

    public void DoneButtonClick()
    {
        //Taptic.Light();

        //winScreen_LevelEndMoneyText.text = PlayerManager.Instance.level.ToString();

        //inGameScreen.gameObject.SetActive(false);
        //winScreen.gameObject.SetActive(true);

        //Sequence sequence = DOTween.Sequence();
        //float totalDuration = 26 * .05f;

        //for (int i = 1; i < 26; i++)
        //{
        //    GameObject moneyImageGO = ObjectPooler.Instance
        //        .SpawnFromPool("levelEndMoneyImage", winScreen_incomeBG.Find("MoneyImage").position, Quaternion.identity, false);
        //    RectTransform moneyImageTransform = moneyImageGO.GetComponent<RectTransform>();

        //    Tween tween = moneyImageTransform.DOMove(winScreen_moneyImageRef.position, i * .05f)
        //        .SetEase(Ease.InBack)
        //        .OnStart(() =>
        //        {
        //            moneyImageTransform.localScale = Vector3.one * 1.5f;
        //            moneyImageGO.SetActive(true);
        //        })
        //        .OnComplete(() =>
        //        {
        //            Taptic.Light();
        //            moneyImageTransform.gameObject.SetActive(false);
        //        });

        //    sequence.Join(tween);
        //}

        //sequence.SetDelay(1.5f)
        //    .SetEase(Ease.Linear)
        //    .OnComplete(() => nextButton.gameObject.SetActive(true));

        //StartCoroutine(UpdateMoneyDelayed(PlayerManager.Instance.level, totalDuration / (PlayerManager.Instance.level)));

        //GameManager.Instance.EndLevel();
    }

    public void NextLevelButtonClick()
    {
        //Taptic.Light();

        //GameManager.Instance.NextLevel();
    }

    #endregion

    public TargetIndicator AddTargetIndicator(GameObject target, Color color, Sprite inScreenImage, Sprite offScreenImage)
    {
        Canvas canvas = GetComponent<Canvas>();
        TargetIndicator indicator = Instantiate(TargetIndicatorPrefab, canvas.transform).GetComponent<TargetIndicator>();
        indicator.InitialiseTargetIndicator(target, Camera.main, GetComponent<Canvas>());
        indicator.SetImageAndColors(color, inScreenImage, offScreenImage);
        indicator.transform.SetParent(targetIndicatorsParent, false);
        targetIndicators.Add(indicator);

        return indicator;
    }

    #region Upgrade
    public void OpenPlayerUpgradePanel()
    {
        PlayerController player = PlayerController.Instance;

        if (player.speedLevel < player.settings.speeds.Count)
        {
            if (GameManager.Instance.currentMoney >= player.settings.GetSpeedUpgradePrice(player.speedLevel))
            {
                playerSpeedUpgradeButton.enabled = true;
                playerSpeedButtonMoneyImage.gameObject.SetActive(true);
                playerSpeedUpgradeTypeText.text = "Speed " + player.speedLevel;
                playerSpeedUpgradePriceText.text = player.settings.GetSpeedUpgradePrice(player.speedLevel).ToString();
                playerSpeedUpgradeButtonDisabled.gameObject.SetActive(false);
                playerSpeedButtonMoneyImage.gameObject.SetActive(true);
                playerSpeedUpgradeButton.gameObject.SetActive(true);
            }
            else
            {
                playerSpeedUpgradeTypeTextDisabled.text = "Speed " + player.speedLevel;
                playerSpeedUpgradePriceTextDisabled.text = player.settings.GetSpeedUpgradePrice(player.speedLevel).ToString();
                playerSpeedUpgradeButton.gameObject.SetActive(false);
                playerSpeedUpgradeButtonDisabled.gameObject.SetActive(true);
            }
        }
        else
        {
            playerSpeedUpgradeButton.enabled = false;
            playerSpeedButtonMoneyImage.gameObject.SetActive(false);
            playerSpeedUpgradeTypeText.text = "Max Speed";
            playerSpeedUpgradePriceText.text = "";
        }

        if (player.capacityLevel < player.settings.capacities.Count)
        {
            if (GameManager.Instance.currentMoney >= player.settings.GetCapacityUpgradePrice(player.capacityLevel))
            {
                playerCapacityUpgradeButton.enabled = true;
                playerCapacityButtonMoneyImage.gameObject.SetActive(true);
                playerCapacityUpgradeTypeText.text = "Capacity " + player.capacityLevel;
                playerCapacityUpgradePriceText.text = player.settings.GetCapacityUpgradePrice(player.capacityLevel).ToString();
                playerCapacityUpgradeButtonDisabled.gameObject.SetActive(false);
                playerCapacityButtonMoneyImage.gameObject.SetActive(true);
                playerCapacityUpgradeButton.gameObject.SetActive(true);
            }
            else
            {
                playerCapacityUpgradeTypeTextDisabled.text = "Capacity " + player.capacityLevel;
                playerCapacityUpgradePriceTextDisabled.text = player.settings.GetCapacityUpgradePrice(player.capacityLevel).ToString();
                playerCapacityUpgradeButton.gameObject.SetActive(false);
                playerCapacityUpgradeButtonDisabled.gameObject.SetActive(true);
            }
        }
        else
        {
            playerCapacityUpgradeButton.enabled = false;
            playerCapacityButtonMoneyImage.gameObject.SetActive(false);
            playerCapacityUpgradeTypeText.text = "Max Capacity";
            playerCapacityUpgradePriceText.text = "";
        }

        currentPanel = playerUpgradePanel;
        playerUpgradePanel.SetActive(true);
    }

    public void OnPlayerSpeedUpgradeButtonClick()
    {
        PlayerController player = PlayerController.Instance;
        int currentMoney = GameManager.Instance.currentMoney;
        int upgradePrice = player.settings.GetSpeedUpgradePrice(player.speedLevel);

        if (player.capacityLevel + 1 < player.settings.capacities.Count && currentMoney - upgradePrice < player.settings.GetCapacityUpgradePrice(player.capacityLevel))
        {
            playerCapacityUpgradeTypeTextDisabled.text = "Capacity " + player.capacityLevel;
            playerCapacityUpgradePriceTextDisabled.text = player.settings.GetCapacityUpgradePrice(player.capacityLevel).ToString();
            playerCapacityUpgradeButton.gameObject.SetActive(false);
            playerCapacityUpgradeButtonDisabled.gameObject.SetActive(true);
        }

        if (player.speedLevel + 1 < player.settings.speeds.Count)
        {
            int nextUpgradePrice = player.settings.GetSpeedUpgradePrice(player.speedLevel + 1);
            if (currentMoney - upgradePrice >= nextUpgradePrice)
            {
                playerSpeedUpgradeButton.enabled = true;
                playerSpeedUpgradeTypeText.text = "Speed " + (player.speedLevel + 1);
                playerSpeedUpgradePriceText.text = nextUpgradePrice.ToString();
                playerSpeedUpgradeButtonDisabled.gameObject.SetActive(false);
                playerSpeedButtonMoneyImage.gameObject.SetActive(true);
                playerSpeedUpgradeButton.gameObject.SetActive(true);
            }
            else
            {
                playerSpeedUpgradeTypeTextDisabled.text = "Speed " + (player.speedLevel + 1);
                playerSpeedUpgradePriceTextDisabled.text = nextUpgradePrice.ToString();
                playerSpeedUpgradeButton.gameObject.SetActive(false);
                playerSpeedUpgradeButtonDisabled.gameObject.SetActive(true);
            }
        }
        else
        {
            playerSpeedUpgradeButton.enabled = false;
            playerSpeedButtonMoneyImage.gameObject.SetActive(false);
            playerSpeedUpgradeTypeText.text = "Max Speed";
            playerSpeedUpgradePriceText.text = "";
        }

        GameManager.Instance.UpdateMoney(-upgradePrice);
        PlayerController.Instance.UpgradeSpeed();

        ElephantSDK.Elephant.Event("PlayerSpeed_Upgrade", player.speedLevel);

        Taptic.Light();
    }

    public void OnPlayerCapacityUpgradeButtonClick()
    {
        PlayerController player = PlayerController.Instance;
        int currentMoney = GameManager.Instance.currentMoney;
        int upgradePrice = player.settings.GetCapacityUpgradePrice(player.capacityLevel);

        if (player.speedLevel + 1 < player.settings.speeds.Count && currentMoney - upgradePrice < player.settings.GetSpeedUpgradePrice(player.speedLevel))
        {
            playerSpeedUpgradeTypeTextDisabled.text = "Speed " + player.speedLevel;
            playerSpeedUpgradePriceTextDisabled.text = player.settings.GetSpeedUpgradePrice(player.speedLevel).ToString();
            playerSpeedUpgradeButton.gameObject.SetActive(false);
            playerSpeedUpgradeButtonDisabled.gameObject.SetActive(true);
        }

        if (player.capacityLevel + 1 < player.settings.capacities.Count)
        {
            int nextUpgradePrice = player.settings.GetCapacityUpgradePrice(player.capacityLevel + 1);
            if (currentMoney - upgradePrice >= nextUpgradePrice)
            {
                playerCapacityUpgradeButton.enabled = true;
                playerCapacityButtonMoneyImage.gameObject.SetActive(true);
                playerCapacityUpgradeTypeText.text = "Capacity " + (player.capacityLevel + 1);
                playerCapacityUpgradePriceText.text = nextUpgradePrice.ToString();
                playerCapacityUpgradeButtonDisabled.gameObject.SetActive(false);
                playerCapacityButtonMoneyImage.gameObject.SetActive(true);
                playerCapacityUpgradeButton.gameObject.SetActive(true);
            }
            else
            {
                playerCapacityUpgradeTypeTextDisabled.text = "Capacity " + (player.capacityLevel + 1);
                playerCapacityUpgradePriceTextDisabled.text = nextUpgradePrice.ToString();
                playerCapacityUpgradeButton.gameObject.SetActive(false);
                playerCapacityUpgradeButtonDisabled.gameObject.SetActive(true);
            }
        }
        else
        {
            playerCapacityUpgradeButton.enabled = false;
            playerCapacityButtonMoneyImage.gameObject.SetActive(false);
            playerCapacityUpgradeTypeText.text = "Max Capacity";
            playerCapacityUpgradePriceText.text = "";
        }

        GameManager.Instance.UpdateMoney(-upgradePrice);
        PlayerController.Instance.UpgradeCapacity();

        ElephantSDK.Elephant.Event("PlayerCapacity_Upgrade", player.capacityLevel);

        Taptic.Light();
    }

    public void OpenSetupControllerUpgradePanel(ISetupController setup)
    {
        switch (setup.type)
        {
            case SetupControllerType.DonutRawPreparer:

                DonutRawPreparerController donutRawPreparer = (DonutRawPreparerController)setup;
                //donutPreparerCapacityButton.interactable = GameManager.Instance.currentMoney >= donutRawPreparer.settings.GetCapacityUpgradePrice(donutRawPreparer.capacityLevel);

                if (donutRawPreparer.capacityLevel < donutRawPreparer.settings.capacities.Count)
                {
                    if (GameManager.Instance.currentMoney >= donutRawPreparer.settings.GetCapacityUpgradePrice(donutRawPreparer.capacityLevel))
                    {
                        donutPreparerCapacityButton.enabled = true;
                        donutPreparerUpgradeTypeText.text = "Capacity " + donutRawPreparer.capacityLevel;
                        donutPreparerUpgradePriceText.text = donutRawPreparer.settings.GetCapacityUpgradePrice(donutRawPreparer.capacityLevel).ToString();
                        donutPreparerCapacityButtonDisabled.gameObject.SetActive(false);
                        donutPreparerCapacityButtonMoneyImage.gameObject.SetActive(true);
                        donutPreparerCapacityButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        donutPreparerUpgradeTypeTextDisabled.text = "Capacity " + donutRawPreparer.capacityLevel;
                        donutPreparerUpgradePriceTextDisabled.text = donutRawPreparer.settings.GetCapacityUpgradePrice(donutRawPreparer.capacityLevel).ToString();
                        donutPreparerCapacityButtonDisabled.gameObject.SetActive(true);
                        donutPreparerCapacityButtonMoneyImage.gameObject.SetActive(false);
                        donutPreparerCapacityButton.gameObject.SetActive(false);

                    }
                }
                else
                {
                    donutPreparerCapacityButton.enabled = false;
                    donutPreparerCapacityButtonMoneyImage.gameObject.SetActive(false);
                    donutPreparerUpgradeTypeText.text = "Max Capacity";
                    donutPreparerUpgradePriceText.text = "";
                }

                currentSetup = donutRawPreparer;
                currentSetupType = SetupControllerType.DonutRawPreparer;
                currentPanel = donutPreparerUpgradePanel;
                donutPreparerUpgradePanel.SetActive(true);

                break;
            case SetupControllerType.Oven:

                OvenSetupControllerNEW oven = (OvenSetupControllerNEW)setup;

                ovenCapacityButton.interactable = GameManager.Instance.currentMoney >= oven.settings.GetCapacityUpgradePrice(oven.capacityLevel);
                ovenSpeedButton.interactable = GameManager.Instance.currentMoney >= oven.settings.GetTimerUpgradePrice(oven.cookingTimeLevel);


                if (oven.capacityLevel < oven.settings.capacities.Count)
                {
                    if (GameManager.Instance.currentMoney >= oven.settings.GetCapacityUpgradePrice(oven.capacityLevel))
                    {
                        ovenCapacityButton.enabled = true;
                        ovenCapacityUpgradeTypeText.text = "Capacity " + oven.capacityLevel;
                        ovenCapacityUpgradePriceText.text = oven.settings.GetCapacityUpgradePrice(oven.capacityLevel).ToString();
                        ovenCapacityButtonDisabled.gameObject.SetActive(false);
                        ovenCapacityButtonMoneyImage.gameObject.SetActive(true);
                        ovenCapacityButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        ovenCapacityUpgradeTypeTextDisabled.text = "Capacity " + oven.capacityLevel;
                        ovenCapacityUpgradePriceTextDisabled.text = oven.settings.GetCapacityUpgradePrice(oven.capacityLevel).ToString();
                        ovenCapacityButton.gameObject.SetActive(false);
                        ovenCapacityButtonDisabled.gameObject.SetActive(true);
                    }
                }
                else
                {
                    ovenCapacityButton.enabled = false;
                    ovenCapacityButtonMoneyImage.gameObject.SetActive(false);
                    ovenCapacityUpgradeTypeText.text = "Max Capacity";
                    ovenCapacityUpgradePriceText.text = "";
                }

                if (oven.cookingTimeLevel < oven.settings.timers.Count)
                {
                    if (GameManager.Instance.currentMoney >= oven.settings.GetTimerUpgradePrice(oven.cookingTimeLevel))
                    {
                        ovenSpeedButton.enabled = true;
                        ovenSpeedUpgradeTypeText.text = "Speed " + oven.cookingTimeLevel;
                        ovenSpeedUpgradePriceText.text = oven.settings.GetTimerUpgradePrice(oven.cookingTimeLevel).ToString();
                        ovenSpeedButtonDisabled.gameObject.SetActive(false);
                        ovenSpeedButtonMoneyImage.gameObject.SetActive(true);
                        ovenSpeedButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        ovenSpeedUpgradeTypeTextDisabled.text = "Speed " + oven.cookingTimeLevel;
                        ovenSpeedUpgradePriceTextDisabled.text = oven.settings.GetTimerUpgradePrice(oven.cookingTimeLevel).ToString();
                        ovenSpeedButton.gameObject.SetActive(false);
                        ovenSpeedButtonDisabled.gameObject.SetActive(true);
                    }
                }
                else
                {
                    ovenSpeedButton.enabled = false;
                    ovenSpeedButtonMoneyImage.gameObject.SetActive(false);
                    playerSpeedButtonMoneyImage.gameObject.SetActive(false);
                    ovenSpeedUpgradeTypeText.text = "Max Speed";
                    ovenSpeedUpgradePriceText.text = "";
                }

                currentSetup = oven;
                currentSetupType = SetupControllerType.Oven;
                currentPanel = ovenUpgradePanel;
                ovenUpgradePanel.SetActive(true);

                break;
            case SetupControllerType.SauceSpiller:
                break;
        }
    }

    public void OnDonutRawUpgradeButtonClick()
    {
        DonutRawPreparerController donutRawPreparer = (DonutRawPreparerController)currentSetup;

        int currentMoney = GameManager.Instance.currentMoney;
        int upgradePrice = donutRawPreparer.settings.GetCapacityUpgradePrice(donutRawPreparer.capacityLevel);

        if (donutRawPreparer.capacityLevel < donutRawPreparer.settings.capacities.Count)
        {
            int nextUpgradePrice = donutRawPreparer.settings.GetCapacityUpgradePrice(donutRawPreparer.capacityLevel + 1);
            if (currentMoney - upgradePrice >= nextUpgradePrice)
            {
                donutPreparerCapacityButton.enabled = true;
                donutPreparerUpgradeTypeText.text = "Capacity " + (donutRawPreparer.capacityLevel + 1);
                donutPreparerUpgradePriceText.text = nextUpgradePrice.ToString();
                donutPreparerCapacityButtonDisabled.gameObject.SetActive(false);
                donutPreparerCapacityButton.gameObject.SetActive(true);
                donutPreparerCapacityButtonMoneyImage.gameObject.SetActive(true);
            }
            else
            {
                donutPreparerUpgradeTypeTextDisabled.text = "Capacity " + (donutRawPreparer.capacityLevel + 1);
                donutPreparerUpgradePriceTextDisabled.text = nextUpgradePrice.ToString();
                donutPreparerCapacityButton.gameObject.SetActive(false);
                donutPreparerCapacityButtonDisabled.gameObject.SetActive(true);
                donutPreparerCapacityButtonMoneyImage.gameObject.SetActive(false);
            }
        }
        else
        {
            donutPreparerCapacityButton.enabled = false;
            donutPreparerCapacityButtonMoneyImage.gameObject.SetActive(false);
            donutPreparerUpgradeTypeText.text = "Max Capacity";
            donutPreparerUpgradePriceText.text = "";
        }

        GameManager.Instance.UpdateMoney(-upgradePrice);
        donutRawPreparer.UpgradeCapacity();

        ElephantSDK.Elephant.Event("MachineCapacity_Upgrade", donutRawPreparer.capacityLevel);

        Taptic.Light();
    }

    public void OnOvenSpeedUpgradeButtonClick()
    {
        OvenSetupControllerNEW oven = (OvenSetupControllerNEW)currentSetup;

        int currentMoney = GameManager.Instance.currentMoney;
        int upgradePrice = oven.settings.GetTimerUpgradePrice(oven.cookingTimeLevel);

        if (oven.cookingTimeLevel < oven.settings.timers.Count)
        {
            int nextUpgradePrice = oven.settings.GetTimerUpgradePrice(oven.cookingTimeLevel + 1);
            if ((currentMoney - upgradePrice) >= nextUpgradePrice)
            {
                ovenSpeedButton.enabled = true;
                ovenSpeedUpgradeTypeText.text = "Speed " + (oven.cookingTimeLevel + 1);
                ovenSpeedUpgradePriceText.text = oven.settings.GetTimerUpgradePrice(oven.cookingTimeLevel + 1).ToString();
                ovenSpeedButtonDisabled.gameObject.SetActive(false);
                ovenSpeedButtonMoneyImage.gameObject.SetActive(true);
                ovenSpeedButton.gameObject.SetActive(true);
            }
            else
            {
                ovenSpeedUpgradeTypeTextDisabled.text = "Speed " + (oven.cookingTimeLevel + 1);
                ovenSpeedUpgradePriceTextDisabled.text = oven.settings.GetTimerUpgradePrice(oven.cookingTimeLevel + 1).ToString();
                ovenSpeedButton.gameObject.SetActive(false);
                ovenSpeedButtonDisabled.gameObject.SetActive(true);
            }
        }
        else
        {
            ovenSpeedButton.enabled = false;
            ovenSpeedButtonMoneyImage.gameObject.SetActive(false);
            ovenSpeedUpgradeTypeText.text = "Max Speed";
            ovenSpeedUpgradePriceText.text = "";
        }

        GameManager.Instance.UpdateMoney(-upgradePrice);
        oven.UpgradeCookingTime();

        ElephantSDK.Elephant.Event("MachineSpeed_Upgrade", oven.cookingTimeLevel);

        Taptic.Light();
    }

    public void OnOvenCapacityUpgradeButtonClick()
    {
        OvenSetupControllerNEW oven = (OvenSetupControllerNEW)currentSetup;

        int currentMoney = GameManager.Instance.currentMoney;
        int upgradePrice = oven.settings.GetCapacityUpgradePrice(oven.capacityLevel);

        if (oven.capacityLevel < oven.settings.capacities.Count)
        {
            int nextUpgradePrice = oven.settings.GetCapacityUpgradePrice(oven.capacityLevel + 1);
            if ((currentMoney - upgradePrice) >= nextUpgradePrice)
            {
                ovenCapacityButton.enabled = true;
                ovenCapacityUpgradeTypeText.text = "Capacity " + (oven.capacityLevel + 1);
                ovenCapacityUpgradePriceText.text = oven.settings.GetCapacityUpgradePrice(oven.capacityLevel + 1).ToString();
                ovenCapacityButtonDisabled.gameObject.SetActive(false);
                ovenCapacityButtonMoneyImage.gameObject.SetActive(true);
                ovenCapacityButton.gameObject.SetActive(true);
            }
            else
            {
                ovenCapacityUpgradeTypeTextDisabled.text = "Capacity " + (oven.capacityLevel + 1);
                ovenCapacityUpgradePriceTextDisabled.text = oven.settings.GetCapacityUpgradePrice(oven.capacityLevel + 1).ToString();
                ovenCapacityButton.gameObject.SetActive(false);
                ovenCapacityButtonDisabled.gameObject.SetActive(true);
            }
        }
        else
        {
            ovenCapacityButton.enabled = false;
            ovenCapacityButtonMoneyImage.gameObject.SetActive(false);
            ovenCapacityUpgradeTypeText.text = "Max Capacity";
            ovenCapacityUpgradePriceText.text = "";
        }

        GameManager.Instance.UpdateMoney(-upgradePrice);
        oven.UpgradeCapacity();

        ElephantSDK.Elephant.Event("MachineCapacity_Upgrade", oven.capacityLevel);

        Taptic.Light();
    }

    public void CloseUpgradePanel()
    {
        if (currentPanel)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
            currentSetup = null;
            currentSetupType = SetupControllerType.None;
        }
    }

    #endregion
}
