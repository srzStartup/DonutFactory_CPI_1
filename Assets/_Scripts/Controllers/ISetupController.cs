using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISetupController
{
    public int id { get; set; }
    public Transform cachedTransform { get; set; }
    public ControllerSettings Settings { get; }
    public SetupControllerType type { get; }
    public bool IsUnlocked { get; set; }
    public int remainToUnlock { get; set; }
    public LockSpriteController LockSprite { get; }

    void Unlock();

    void TriggerUpgrade();
}
