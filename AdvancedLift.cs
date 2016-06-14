// Decompiled with JetBrains decompiler
// Type: AdvancedLift
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0C6BE4A-6FED-4DEE-8B70-659B873D0CEE
// Assembly location: F:\Games\Steam\steamapps\common\FortressCraft\64\FC_64_Data\Managed\Assembly-CSharp.dll

using System.IO;
using UnityEngine;

public class AdvancedLift : MachineEntity
{
    public int mnExtendRange = 64;
    public bool mbRestAtBottom = true;
    public float mrMaxStoredGas = 4f;
    public float mrMaxGas = 24f;
    public int mnMaxPossibleExtend = 256;
    public int mnPlayerOcclusionSpace = 2;
    public int mnMaxExtendWithoutCompressor = 64;
    public int mnTravelDist = 256;
    public int mnSafeMaxExtend = 256;
    private int mnColliderRef = -1;
    private int mnColliderPistonRef = -1;
    private float mrMaxSpeed = 12f;
    private float mrMaxEmptySpeed = 32f;
    private float mrMaxNoGasSpeed = 3f;
    private float mrEmptyAcceleration = 6f;
    private float mrRegularAcceleration = 2f;
    private float mrManualAcceleration = 3f;
    private float mrMaxPauseTime = 5f;
    private float mrBaseGasRegen = 0.025f;
    private float mrGasUsageRate = 1f;
    private float mrNearbyAbove = 8f;
    private float mrNearbyBelow = 5f;
    private float mrNearbyHorizontal = 5f;
    private float mrOnLiftDist = 1.3f;
    private float mrCallLiftDist = 8.05f;
    private bool mbUnityEntityNeedsUpdating;
    private int mnLFUpdates;
    public bool mbLinkedToGO;
    public bool mbManualControlAttached;
    private bool mbStopSFXQueued;
    public float mrStoredGas;
    public bool[,,] shaftClearance;
    public bool[] levelClearance;
    public int mnRoundRobin;
    public float mrCurrentExtend;
    public int mnSafeMinExtend;
    public bool mbExtended;
    public float mrTimeSinceLastPoweredCompressorNotification;
    [HideInInspector]
    public float mrCurrentGas;
    public bool mbManuallyControlled;
    public float mrSpeed;
    private bool mbControllingPlayer;
    public bool mbPlayerIsNearby;
    private AdvancedLift.eLiftDirection meCurrentDirection;
    private AdvancedLift.eLiftState meState;
    public bool mbHasPoweredCompressor;
    private float mrPauseTimer;
    private float mrLiftDestination;
    private float mrDesiredLiftDestination;
    public AdvLiftEntity mLiftEntity;
    private float mrStartAudioDebounce;
    private AudioSource AudioTravelLoop;
    private AudioSource mWorkAudio;
    private AudioSource mArriveAudio;
    private MaterialPropertyBlock mMPB;
    private int mnExtendMultiplier = 1;
    private int mnContractMultiplier = 1;
    private float mrExtendMultiplierTimer;
    private float mrContractMultiplierTimer;
    private bool firstrunholobase = true;

    public AdvancedLift(Segment segment, long x, long y, long z, ushort cube, byte flags, ushort lValue)
      : base(eSegmentEntity.Mod, SpawnableObjectEnum.Lift_T1, x, y, z, cube, flags, lValue, Vector3.zero, segment)
    {
        this.shaftClearance = new bool[3, this.mnMaxPossibleExtend + 1 + this.mnPlayerOcclusionSpace, 3];
        this.levelClearance = new bool[this.mnMaxPossibleExtend + 1 + this.mnPlayerOcclusionSpace];
        this.mbNeedsLowFrequencyUpdate = true;
        this.mbNeedsUnityUpdate = true;
        this.mrMaxGas = 24f;
        this.mrCurrentGas = this.mrMaxGas;
        if (WorldScript.mbIsServer && WorldScript.instance.mnLoadDistance < 8)
            this.mnMaxExtendWithoutCompressor = 32;
        if (this.mSegment.mbIsQueuedForUpdate)
            return;
        WorldScript.instance.mSegmentUpdater.AddSegment(this.mSegment);
        if (Holobase.mbBaseActive)
        {
            HoloMachineEntity holo = this.CreateHolobaseEntity(Holobase.instance);
            Holobase.instance.maMachines.Add(holo);
            holo.VisualisationObjects[0].SetActive(true);
            holo.VisualisationObjects[0].SetActive(true);
        }
    }

    public void AlterExtendRange(bool lbFineControl)
    {
        if (lbFineControl)
            ++this.mnExtendRange;
        else
            this.mnExtendRange += 8;
        if (this.mnExtendRange > this.mnMaxPossibleExtend)
            this.mnExtendRange = 8;
        this.mbUnityEntityNeedsUpdating = true;
        this.MarkDirtyDelayed();
        this.RequestImmediateNetworkUpdate();
    }

    public void SetRange(int newRange)
    {
        if (newRange < 0)
            newRange = 0;
        if (((int)mFlags & 63) == 2)
            if (newRange < 2)
                newRange = 2;
        if (newRange > this.mnMaxPossibleExtend)
            newRange = this.mnMaxPossibleExtend;
        this.mnExtendRange = newRange;
        this.mbUnityEntityNeedsUpdating = true;
        this.MarkDirtyDelayed();
        this.RequestImmediateNetworkUpdate();
    }

    public void FlipExtendType()
    {
        this.mbRestAtBottom = !this.mbRestAtBottom;
        this.mbUnityEntityNeedsUpdating = true;
        this.MarkDirtyDelayed();
    }

    public override void DropGameObject()
    {
        if ((Object)this.mLiftEntity != (Object)null)
        {
            this.mLiftEntity.DroppingEntity();
            this.mLiftEntity = (AdvLiftEntity)null;
        }
        base.DropGameObject();
        this.mbLinkedToGO = false;
        this.HandleLiftRemoved();
    }

    public override void OnDelete()
    {
        if (!object.ReferenceEquals((object)this.mLiftEntity, (object)null))
        {
            this.mLiftEntity.DroppingEntity();
            this.mLiftEntity = (AdvLiftEntity)null;
            this.HandleLiftRemoved();
        }
        base.OnDelete();
    }

    public override bool ShouldSave()
    {
        return true;
    }

    public override void Write(BinaryWriter writer)
    {
        float num = 0.0f;
        writer.Write(this.mnExtendRange);
        writer.Write(this.mbRestAtBottom);
        writer.Write(this.mrCurrentExtend);
        writer.Write(this.mrCurrentGas);
        writer.Write(this.mnSafeMinExtend);
        writer.Write(this.mnSafeMaxExtend);
        writer.Write(num);
        writer.Write(num);
        writer.Write(num);
    }

    public override void Read(BinaryReader reader, int entityVersion)
    {
        this.mnExtendRange = reader.ReadInt32();
        this.mbRestAtBottom = reader.ReadBoolean();
        this.mrCurrentExtend = reader.ReadSingle();
        this.mrCurrentGas = reader.ReadSingle();
        this.mnSafeMinExtend = reader.ReadInt32();
        this.mnSafeMaxExtend = reader.ReadInt32();
        if (this.mnSafeMinExtend < 0)
            this.mnSafeMinExtend = 0;
        if (this.mnSafeMaxExtend > this.mnMaxPossibleExtend)
            this.mnSafeMaxExtend = this.mnMaxPossibleExtend;
        if (!WorldScript.mbIsServer)
        {
            if (this.mnSafeMaxExtend > 64)
                this.mnSafeMaxExtend = 64;
            if ((double)this.mrCurrentExtend > 64.0)
                this.mrCurrentExtend = 64f;
        }
        for (int index1 = this.mnSafeMinExtend; index1 < this.mnSafeMaxExtend + 1 + this.mnPlayerOcclusionSpace; ++index1)
        {
            this.levelClearance[index1] = true;
            for (int index2 = 0; index2 < 3; ++index2)
            {
                for (int index3 = 0; index3 < 3; ++index3)
                    this.shaftClearance[index2, index1, index3] = true;
            }
        }
        double num1 = (double)reader.ReadSingle();
        double num2 = (double)reader.ReadSingle();
        double num3 = (double)reader.ReadSingle();
        this.mbUnityEntityNeedsUpdating = true;
    }

    public override void WriteNetworkUpdate(BinaryWriter writer)
    {
        writer.Write(this.mnExtendRange);
    }

    public override void ReadNetworkUpdate(BinaryReader reader)
    {
        this.mnExtendRange = reader.ReadInt32();
        this.mbUnityEntityNeedsUpdating = true;
    }

    public override void UnitySuspended()
    {
    }

    public override string GetPopupText()
    {

        AdvancedLift lift = (AdvancedLift)WorldScript.instance.localPlayerInstance.mPlayerBlockPicker.selectedEntity;
        if (lift == null)
            return (string) null;
        if (Input.GetButtonDown("Extend Lift Range") && UIManager.AllowInteracting)
        {
            int newRange = lift.mnExtendRange + 1;
            if (Input.GetKey(KeyCode.LeftShift))
                newRange = lift.mnExtendRange + 10;
            AdvLiftWindow.SetRange(WorldScript.mLocalPlayer, lift, newRange);
            this.mrExtendMultiplierTimer = 0.0f;
            this.mnExtendMultiplier = 1;
        }
        else if (Input.GetButton("Extend Lift Range"))
        {
            this.mrExtendMultiplierTimer += Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftShift))
                this.mnExtendMultiplier = 16;
            while ((double)this.mrExtendMultiplierTimer > 0.5)
            {
                this.mrExtendMultiplierTimer -= 0.5f;
                ++this.mnExtendMultiplier;
                if (this.mnExtendMultiplier > 16)
                    this.mnExtendMultiplier = 16;
                AdvLiftWindow.SetRange(WorldScript.mLocalPlayer, lift, lift.mnExtendRange + this.mnExtendMultiplier);
            }
        }
        if (Input.GetButtonDown("Extract") && UIManager.AllowInteracting)
        {
            int newRange = lift.mnExtendRange - 1;
            if (Input.GetKey(KeyCode.LeftShift))
                newRange = lift.mnExtendRange - 10;
            if (newRange < 0)
                newRange = 0;
            AdvLiftWindow.SetRange(WorldScript.mLocalPlayer, lift, newRange);
            this.mrContractMultiplierTimer = 0.0f;
            this.mnContractMultiplier = 1;
        }
        else if (Input.GetButton("Extract"))
        {
            this.mrContractMultiplierTimer += Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftShift))
                this.mnContractMultiplier = 16;
            while ((double)this.mrContractMultiplierTimer > 0.5)
            {
                this.mrContractMultiplierTimer -= 0.5f;
                ++this.mnContractMultiplier;
                if (this.mnContractMultiplier > 16)
                    this.mnContractMultiplier = 16;
                AdvLiftWindow.SetRange(WorldScript.mLocalPlayer, lift, lift.mnExtendRange - this.mnContractMultiplier);
            }
        }
        string lStr1 = "Tier 1 Advanced Central Piston Lift" + (object)"\nCurrent Range: " + (object)lift.mnExtendRange + "\nAdjust range by holding (Q) or (E).";
        if (!lift.mbHasPoweredCompressor)
            lStr1 = string.Concat(new object[4]
            {
        (object) lStr1,
        (object) "\nPowered compressor required beyond ",
        (object) lift.mnMaxExtendWithoutCompressor,
        (object) "m"
            });
        if (!WorldScript.mbIsServer && lift.mnExtendRange > 64)
            lStr1 += "\nLifts limited to 64m for network clients. Sorry!";
        if (lift.mbLinkedToGO && (UnityEngine.Object)lift.mLiftEntity != (UnityEngine.Object)null)
        {
            string str1 = lStr1 + "\nCurrent Pressure : " + lift.mrCurrentGas.ToString("F2") + "/" + lift.mrMaxGas.ToString("F2") + "psi";
            string str2;
            if (lift.mbManualControlAttached)
                str2 = str1 + "\nLift is under Manual Control";
            else
                str2 = string.Concat(new object[4]
                {
          (object) str1,
          (object) "\nStored Pressure : ",
          (object) lift.mrStoredGas.ToString("F2"),
          (object) "psi"
                });
            lStr1 = string.Concat(new object[4]
            {
        (object) str2,
        (object) "\nCurrent Speed : ",
        (object) lift.mrSpeed,
        (object) "m/s",
            });
        }
        return lStr1;
    }

    public override void OnUpdateRotation(byte newFlags)
    {
        int x = (int)(this.mnX - mSegment.baseX);
        int y = (int)(this.mnY - mSegment.baseY);
        int z = (int)(this.mnZ - mSegment.baseZ);

        // nodeworker automatically sets the new flags in the cubedata that is actually used by the diskserializer, so we have to restore them!
        mSegment.maCubeData[(y << 8) + (z << 4) + x].meFlags = this.mFlags;
    }

    public override void UnityUpdate()
    {
        this.mrStartAudioDebounce -= Time.deltaTime;
        if (!this.mbLinkedToGO)
        {
            if (this.mWrapper == null || this.mWrapper.mGameObjectList == null)
                return;
            if ((Object)this.mWrapper.mGameObjectList[0].gameObject == (Object)null)
                Debug.LogError((object)"Lift missing game object #0 (GO)?");

            var old = mWrapper.mGameObjectList[0].gameObject.GetComponentInChildren<LiftEntityUnity>();
            this.mLiftEntity = old.gameObject.AddComponent<AdvLiftEntity>();
            this.mLiftEntity.PistonObject = old.PistonObject;
            GameObject.Destroy(old);

            this.mLiftEntity.mMachineEntity = this;
            this.mbUnityEntityNeedsUpdating = true;
            this.AudioTravelLoop = Extensions.Search(this.mWrapper.mGameObjectList[0].transform, "AudioTravelLoop").GetComponent<AudioSource>();
            this.mArriveAudio = Extensions.Search(this.mWrapper.mGameObjectList[0].transform, "AudioArrive").GetComponent<AudioSource>();
            this.mMPB = new MaterialPropertyBlock();
            this.mbLinkedToGO = true;
        }
        else
        {
            if (this.mbUnityEntityNeedsUpdating)
            {
                this.mbManuallyControlled = this.mbManualControlAttached;
                this.mnTravelDist = this.mnExtendRange;
                this.UpdateSafeExtents();
                this.mbUnityEntityNeedsUpdating = false;
            }
            this.mbManuallyControlled = this.mbManualControlAttached;
            if ((double)this.mrStoredGas > 0.100000001490116 && (double)this.mrCurrentGas <= (double)this.mrMaxGas - 0.100000001490116)
            {
                this.mrCurrentGas += 0.1f;
                this.mrStoredGas -= 0.1f;
            }
            this.mMPB.Clear();
            this.mMPB.AddFloat("_GlowMult", Mathf.Abs(this.mrSpeed / 1f));
            this.mLiftEntity.GetComponent<Renderer>().SetPropertyBlock(this.mMPB);
            if ((double)this.mDistanceToPlayer < 32.0)
            {
                float num1 = Mathf.Abs(this.mrSpeed / 12f);
                float num2 = 0.35f + num1;
                if ((double)num1 < 0.00999999977648258)
                {
                    if (!this.AudioTravelLoop.isPlaying)
                        return;
                    this.AudioTravelLoop.Stop();
                }
                else if ((double)num1 == 0.0)
                {
                    if ((double)this.AudioTravelLoop.pitch <= 0.349999994039536)
                    {
                        if (!this.AudioTravelLoop.isPlaying)
                            return;
                        this.AudioTravelLoop.Stop();
                    }
                    else
                    {
                        this.AudioTravelLoop.pitch *= 0.95f;
                        this.AudioTravelLoop.volume *= 0.95f;
                    }
                }
                else
                {
                    if (!this.AudioTravelLoop.isPlaying)
                    {
                        this.AudioTravelLoop.Play();
                        this.AudioTravelLoop.pitch = num2;
                        this.AudioTravelLoop.volume = 0.0f;
                    }
                    this.AudioTravelLoop.pitch += (num2 - this.AudioTravelLoop.pitch) * Time.deltaTime;
                    float num3 = num1 * 2f;
                    if ((double)num3 > 1.0)
                        num3 = 1f;
                    this.AudioTravelLoop.volume = num3;
                }
            }
            else
            {
                if (!this.AudioTravelLoop.isPlaying)
                    return;
                this.AudioTravelLoop.volume *= 0.9f;
                if ((double)this.AudioTravelLoop.volume >= 0.100000001490116)
                    return;
                this.AudioTravelLoop.Stop();
            }
        }
    }

    public override bool HasLongRangeGraphics()
    {
        return true;
    }

    public override void LowFrequencyUpdate()
    {
        this.mrTimeSinceLastPoweredCompressorNotification += 0.2f;
        if ((double)this.mrTimeSinceLastPoweredCompressorNotification > 30.0 && this.mbHasPoweredCompressor)
        {
            this.mbHasPoweredCompressor = false;
            this.mbUnityEntityNeedsUpdating = true;
        }
        else if ((double)this.mrTimeSinceLastPoweredCompressorNotification <= 30.0 && !this.mbHasPoweredCompressor)
        {
            this.mbHasPoweredCompressor = true;
            this.mbUnityEntityNeedsUpdating = true;
        }
        bool flag1 = false;
        if (object.ReferenceEquals((object)this.mLiftEntity, (object)null) && this.mWrapper == null)
            this.SpawnGameObject();
        bool flag2 = this.mbPlayerIsNearby;
        int num1 = !flag2 ? this.mnRoundRobin : 0;
        int num2 = !flag2 ? this.mnRoundRobin : this.mnMaxPossibleExtend + 1 + this.mnPlayerOcclusionSpace;
        long x;
        long y;
        long z;
        int index1;
        int index2;
        int index3;
        
        //Determine shaft clearance
        for (index1 = num1; index1 < num2; ++index1)
                {
                    for (index2 = 0; index2 < 3; ++index2)
                    {
                        for (index3 = 0; index3 < 3; ++index3)
                        {
                            switch ((int)mFlags & 63)
                            {
                                //+y is extended case
                                case 1:
                                    x = this.mnX + (long)index2 - 1L;
                                    y = this.mnY + (long)index1 + 1L;
                                    z = this.mnZ + (long)index3 - 1L;
                                    break;
                                //-y case
                                case 2:
                                    x = this.mnX + (long)index2 - 1L;
                                    y = this.mnY - (long)index1 - 1L;
                                    z = this.mnZ + (long)index3 - 1L;
                                    break;
                                //+z case
                                case 4:
                                    x = this.mnX + (long)index2 - 1L;
                                    y = this.mnY + (long)index3;
                                    z = this.mnZ + (long)index1 + 1L;
                                    break;
                                //-z case
                                case 8:
                                    x = this.mnX + (long)index2 - 1L;
                                    y = this.mnY + (long)index3;
                                    z = this.mnZ - (long)index1 - 1L;
                                    break;
                                //+x case
                                case 16:
                                    x = this.mnX + (long)index1 + 1L;
                                    y = this.mnY + (long)index2;
                                    z = this.mnZ + (long)index3 - 1L;
                                    break;
                                //-x case
                                case 32:
                                    x = this.mnX - (long)index1 - 1L;
                                    y = this.mnY + (long)index2;
                                    z = this.mnZ + (long)index3 - 1L;
                                    break;
                                //old default lift
                                default:
                                    x = this.mnX + (long)index2 - 1L;
                                    y = this.mnY + (long)index1 + 1L;
                                    z = this.mnZ + (long)index3 - 1L;
                                    break;
                            }
                            Segment segment = this.AttemptGetSegment(x, y, z);
                            if (segment != null && segment.mbInitialGenerationComplete && !segment.mbDestroyed)
                            {
                                bool flag3 = CubeHelper.IsTypeConsideredPassable((int)segment.GetCube(x, y, z));
                                if (this.shaftClearance[index2, index1, index3] != flag3)
                                {
                                    this.shaftClearance[index2, index1, index3] = flag3;
                                    bool flag4 = true;
                                    //check all blocks at this level to determine if we can travel another meter and flag accordingly
                                    for (int index4 = 0; index4 < 3; ++index4)
                                    {
                                        for (int index5 = 0; index5 < 3; ++index5)
                                        {
                                            if (!this.shaftClearance[index4, index1, index5])
                                                flag4 = false;
                                        }
                                    }
                                    if (this.levelClearance[index1] != flag4)
                                    {
                                        this.levelClearance[index1] = flag4;
                                        flag1 = true;
                                    }
                                }
                            }
                        }
                    }
                }
        ++this.mnRoundRobin;
        if (this.mnRoundRobin >= this.mnMaxPossibleExtend + 1 + this.mnPlayerOcclusionSpace)
            this.mnRoundRobin = 0;
        if (!flag1)
            return;
        this.UpdateSafeExtents();
    }

    private void UpdateSafeExtents()
    {
        int num1 = Mathf.FloorToInt(this.mrCurrentExtend);
        if (num1 < 0)
            num1 = 0;
        if (num1 > this.mnMaxPossibleExtend - 1)
            num1 = this.mnMaxPossibleExtend - 1;
        int num2 = num1;
        //Safe Max extent
        switch ((int)mFlags & 63)
        {
            case 1:
                for (int index = num1; index < this.mnMaxPossibleExtend + 1 + this.mnPlayerOcclusionSpace && this.levelClearance[index]; ++index)
                num2 = index - this.mnPlayerOcclusionSpace;
                break;
            case 2:
                for (int index = num1; index < this.mnMaxPossibleExtend + 1 + this.mnPlayerOcclusionSpace && this.levelClearance[index]; ++index)
                num2 = index;
                break;
            case 4:
            case 8:
            case 16:
            case 32:
                for (int index = num1; index < this.mnMaxPossibleExtend + 2 && this.levelClearance[index]; ++index)
                    num2 = index - 1;
                break;
            default:
                for (int index = num1; index < this.mnMaxPossibleExtend + 1 + this.mnPlayerOcclusionSpace && this.levelClearance[index]; ++index)
                num2 = index - this.mnPlayerOcclusionSpace;
                break;
        }
        this.mnSafeMaxExtend = num2 <= this.mnTravelDist ? num2 : this.mnTravelDist;
        if (!this.mbHasPoweredCompressor && this.mnSafeMaxExtend > this.mnMaxExtendWithoutCompressor)
            this.mnSafeMaxExtend = this.mnMaxExtendWithoutCompressor;
        if (!WorldScript.mbIsServer && this.mnSafeMaxExtend > 64)
            this.mnSafeMaxExtend = 64;
        int num3 = num1;
        //Safe min extent
        switch ((int)mFlags & 63)
        {
            case 1:
                for (int index = num1; index >= 0 && this.levelClearance[index]; --index)
                    num3 = index;
                break;
            case 2:
                for (int index = num1; index >= 0 && this.levelClearance[index]; --index)
                    num3 = index + this.mnPlayerOcclusionSpace;
                break;
            case 4:
            case 8:
            case 16:
            case 32:
                for (int index = num1; index >= 0 && this.levelClearance[index]; --index)
                    num3 = index + 1;
                break;
            default:
                for (int index = num1; index >= 0 && this.levelClearance[index]; --index)
                        num3 = index;
                break;
        }
        
        this.mnSafeMinExtend = num3;
        AdvLiftEntity AdvLiftEntity = this.mLiftEntity;
        if (object.ReferenceEquals((object)AdvLiftEntity, (object)null))
            return;
        AdvLiftEntity.ExtentsUpdated();
    }

    private void UnregisterColliders()
    {
        if (this.mnColliderRef >= 0)
            WorldScript.instance.localPlayerInstance.UnregisterCollider(this.mnColliderRef);
        this.mnColliderRef = -1;
        if (this.mnColliderPistonRef >= 0)
            WorldScript.instance.localPlayerInstance.UnregisterCollider(this.mnColliderPistonRef);
        this.mnColliderPistonRef = -1;
    }

    public void HandleLiftRemoved()
    {
        this.UnregisterColliders();
        if (!this.mbControllingPlayer)
            return;
        this.mbControllingPlayer = false;
    }

    public void HandleFixedUpdate()
    {
        if ((Object)this.mLiftEntity == (Object)null || !GameState.PlayerSpawnedAndHadUpdates || WorldScript.instance.localPlayerInstance.mbAbandoned)
            return;
        float num1 = this.meState != AdvancedLift.eLiftState.MovingToPlayer ? this.mrRegularAcceleration : this.mrEmptyAcceleration;
        float num2 = this.meState != AdvancedLift.eLiftState.MovingToPlayer ? this.mrMaxSpeed : this.mrMaxEmptySpeed;
        float currentMaxSpeed = num2;
        if ((double)this.mrCurrentGas <= 0.0)
        {
            this.mrCurrentGas = 0.0f;
            currentMaxSpeed = this.mrMaxNoGasSpeed;
        }
        float lrPlayerFeetY = WorldScript.instance.localPlayerInstance.mPosition.y;
        float lrPlayerFeetX = WorldScript.instance.localPlayerInstance.mPosition.x;
        float lrPlayerFeetZ = WorldScript.instance.localPlayerInstance.mPosition.z;
        Vector3 point = this.mLiftEntity.mStartPos;
                
        float num4 = this.mrSpeed;
        bool flag1 = false;
        //pause timer for turning around the lift
        if ((double)this.mrPauseTimer > 0.0)
        {
            this.mrPauseTimer -= Time.deltaTime;
            if ((double)this.mrPauseTimer <= 0.0)
            {
                this.mrPauseTimer = 0.0f;
                if ((double)this.mrCurrentExtend == (double)this.mnSafeMinExtend && this.meState == AdvancedLift.eLiftState.TransportingPlayer)
                {
                    this.meCurrentDirection = AdvancedLift.eLiftDirection.Upwards;
                    this.SetDestination((float)this.mnTravelDist);
                }
                if ((double)this.mrCurrentExtend == (double)this.mnSafeMaxExtend && this.meState != AdvancedLift.eLiftState.MovingToPlayer)
                {
                    this.meCurrentDirection = AdvancedLift.eLiftDirection.Downwards;
                    this.SetDestination(0.0f);
                }
            }
        }
        else
        {
            //checks to determine if we're outside the bounds of travel
            if ((double)this.mrDesiredLiftDestination > (double)this.mrLiftDestination)
                this.mrLiftDestination = Mathf.Min((float)this.mnSafeMaxExtend, this.mrLiftDestination);
            else if ((double)this.mrDesiredLiftDestination < (double)this.mrLiftDestination)
                this.mrLiftDestination = Mathf.Max((float)this.mnSafeMinExtend, this.mrLiftDestination);
            if (this.meState == AdvancedLift.eLiftState.MovingToPlayer)
                switch((int)mFlags & 63)
                {
                    case 1:
                        this.SetDestination((float)((double)lrPlayerFeetY - (double)point.y - 0.550000011920929));
                        break;
                    case 2:
                        this.SetDestination((float)(-(double)lrPlayerFeetY + (double)point.y + 0.550000011920929));
                        break;
                    case 4:
                        this.SetDestination((float)((double)lrPlayerFeetZ - (double)point.z));
                        break;
                    case 8:
                        this.SetDestination((float)(-(double)lrPlayerFeetZ + (double)point.z));
                        break;
                    case 16:
                        this.SetDestination((float)((double)lrPlayerFeetX - (double)point.x));
                        break;
                    case 32:
                        this.SetDestination((float)(-(double)lrPlayerFeetX + (double)point.x));
                        break;
                    default:
                        this.SetDestination((float)((double)lrPlayerFeetY - (double)point.y - 0.550000011920929));
                        break;
                }
                
            if (this.meState == AdvancedLift.eLiftState.TransportingPlayer && this.mbManuallyControlled)
                this.HandleManualLiftMovement(lrPlayerFeetY, lrPlayerFeetX, lrPlayerFeetZ, currentMaxSpeed);
            else if (this.meState == AdvancedLift.eLiftState.TransportingPlayer || this.meState == AdvancedLift.eLiftState.MovingToPlayer)
            {
                if ((double)this.mrLiftDestination == (double)this.mrCurrentExtend)
                {
                    if ((double)this.mrSpeed > 0.0)
                        //decelerate if at destination
                        this.mrSpeed -= Time.deltaTime * num1;
                    if ((double)this.mrSpeed == 0.0 && this.meState == AdvancedLift.eLiftState.TransportingPlayer)
                    {
                        //handle the turn around point if changing directions
                        if ((double)this.mrCurrentExtend < (double)this.mnSafeMaxExtend)
                        {
                            this.meCurrentDirection = AdvancedLift.eLiftDirection.Upwards;
                            this.SetDestination((float)this.mnTravelDist);
                        }
                        else
                        {
                            this.meCurrentDirection = AdvancedLift.eLiftDirection.Downwards;
                            this.SetDestination(0.0f);
                        }
                    }
                }
                //if we're above the destination
                else if ((double)this.mrLiftDestination < (double)this.mrCurrentExtend)
                {
                    float num5 = this.mrCurrentExtend - this.mrLiftDestination;
                    if (this.meCurrentDirection == AdvancedLift.eLiftDirection.Upwards)
                    {
                        this.mrSpeed -= Time.deltaTime * num1;
                        //if we've turned around change to downwards
                        if ((double)this.mrSpeed <= 0.0)
                        {
                            this.mrSpeed = 0.0f;
                            this.meCurrentDirection = AdvancedLift.eLiftDirection.Downwards;
                        }
                    }
                    else
                    {
                        //approach to destination from above
                        switch ((int)mFlags & 63)
                        {
                            case 1:
                                break;
                            case 2:
                                this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate;
                                break;
                            case 4:
                            case 8:
                            case 16:
                            case 32:
                                this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate * 0.5f;
                                break;
                            default:
                                break;
                        }
                        if ((double)num5 < 12.0 * ((double)this.mrSpeed / (double)num2) && (double)num5 > 0.0)
                            this.mrSpeed += Time.deltaTime * (float)((0.0900000035762787 - (double)Mathf.Pow(this.mrSpeed, 2f)) / (2.0 * (double)num5));
                        else if ((double)this.mrSpeed < (double)currentMaxSpeed)
                        {
                            this.mrSpeed += Time.deltaTime * num1;
                        }
                        else
                        {
                            this.mrSpeed -= Time.deltaTime * num1;
                            if ((double)this.mrSpeed < (double)currentMaxSpeed)
                                this.mrSpeed = currentMaxSpeed;
                        }
                        //handle arrival at destination
                        if ((double)this.mrCurrentExtend - (double)this.mrSpeed * (double)Time.deltaTime < (double)this.mrLiftDestination)
                        {
                            this.mrSpeed = 0.0f;
                            this.mrCurrentExtend = this.mrLiftDestination;
                            flag1 = true;
                            //turn on the pause timer
                            if ((double)this.mrCurrentExtend == (double)this.mnSafeMinExtend && this.meState != AdvancedLift.eLiftState.MovingToPlayer)
                                this.mrPauseTimer = this.mrMaxPauseTime;
                        }
                    }
                }
                //if we're below our destination
                else if ((double)this.mrLiftDestination > (double)this.mrCurrentExtend)
                {
                    float num5 = this.mrLiftDestination - this.mrCurrentExtend;
                    if (this.meCurrentDirection == AdvancedLift.eLiftDirection.Downwards)
                    {
                        this.mrSpeed -= Time.deltaTime * num1;
                        if ((double)this.mrSpeed <= 0.0)
                        {
                            this.mrSpeed = 0.0f;
                            this.meCurrentDirection = AdvancedLift.eLiftDirection.Upwards;
                        }
                    }
                    else
                    {
                        //approaching destination from below
                        switch ((int)mFlags & 63)
                        {
                            case 1:
                                this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate;
                                break;
                            case 2:
                                break;
                            case 4:
                            case 8:
                            case 16:
                            case 32:
                                this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate * 0.5f;
                                break;
                            default:
                                break;
                        }
                        if ((double)num5 < 12.0 * ((double)this.mrSpeed / (double)num2) && (double)num5 > 0.0)
                            this.mrSpeed += Time.deltaTime * (float)((0.0900000035762787 - (double)Mathf.Pow(this.mrSpeed, 2f)) / (2.0 * (double)num5));
                        else if ((double)this.mrSpeed < (double)currentMaxSpeed)
                        {
                            this.mrSpeed += Time.deltaTime * num1;
                        }
                        else
                        {
                            this.mrSpeed -= Time.deltaTime * num1;
                            if ((double)this.mrSpeed < (double)currentMaxSpeed)
                                this.mrSpeed = currentMaxSpeed;
                        }
                        //arrived at destination
                        if ((double)this.mrCurrentExtend + (double)this.mrSpeed * (double)Time.deltaTime > (double)this.mrLiftDestination)
                        {
                            this.mrSpeed = 0.0f;
                            this.mrCurrentExtend = this.mrLiftDestination;
                            flag1 = true;
                            //turn on the pause timer
                            if ((double)this.mrCurrentExtend == (double)this.mnSafeMaxExtend && this.meState != AdvancedLift.eLiftState.MovingToPlayer)
                                this.mrPauseTimer = this.mrMaxPauseTime;
                        }
                    }
                }
            }
            //handle case when lift goes idle (player leaves)
            if (this.meState == AdvancedLift.eLiftState.Idle && (double)this.mrSpeed != 0.0)
                this.mrSpeed -= Time.deltaTime * num1;
            if ((double)this.mrSpeed < 0.0)
                this.mrSpeed = 0.0f;
            if ((double)this.mrSpeed != 0.0 || flag1 || (Object)this.mLiftEntity != (Object)null && this.mLiftEntity.mbFirstRun)
            {
                //handle change in lift extension by adding for speed/time
                if (this.meCurrentDirection == AdvancedLift.eLiftDirection.Upwards)
                    this.mrCurrentExtend += Time.deltaTime * this.mrSpeed;
                else
                    this.mrCurrentExtend -= Time.deltaTime * this.mrSpeed;
                //Tell the lift to move to a safe spot when out of range
                if ((double)this.mrCurrentExtend > (double)this.mnTravelDist)
                {
                    this.SetDestination((float)this.mnTravelDist);
                    //this.mrCurrentExtend = (float)this.mnTravelDist;
                    //this.mrSpeed = 0.0f;
                }
                if ((double)this.mrCurrentExtend > (double)this.mnSafeMaxExtend)
                {
                    //this.mrSpeed = 0.0f;
                    this.SetDestination((float)this.mnSafeMaxExtend);
                    if (this.mbManuallyControlled)
                    {
                        this.mrCurrentExtend = this.mnSafeMaxExtend;
                        this.mrSpeed = 0.0f;
                    }
                }   
                if ((double)this.mrCurrentExtend < 0.0)
                {
                    this.mrCurrentExtend = 0.0f;
                    this.mrSpeed = 0.0f;
                }
                if ((double)this.mrCurrentExtend < (double)this.mnSafeMinExtend)
                {
                    //this.mrSpeed = 0.0f;
                    this.SetDestination((float)this.mnSafeMinExtend);
                    if (this.mbManuallyControlled)
                    {
                        this.mrCurrentExtend = this.mnSafeMinExtend;
                        this.mrSpeed = 0.0f;
                    }
                }    
                //Initialize safe starting position for suspended lift
                if (this.mLiftEntity.mbFirstRun && (((int)mFlags & 63) == 2))
                    this.mrCurrentExtend = 2.0f;
                //Or the lateral lifts
                if (this.mLiftEntity.mbFirstRun && ((((int)mFlags & 63) == 4) || (((int)mFlags & 63) == 8) || (((int)mFlags & 63) == 16) || (((int)mFlags & 63) == 32)))
                    this.mrCurrentExtend = 1.0f;
                //call to update lift positions
                if ((Object)this.mLiftEntity != (Object)null)
                    this.mLiftEntity.UpdatePositions(this.mrCurrentExtend, mFlags);
                this.MarkDirtyDelayed();
            }
        }
        float vo = this.mrSpeed;
        if (this.meCurrentDirection == AdvancedLift.eLiftDirection.Downwards)
            vo = -1f * this.mrSpeed;
        if ((double)num4 < 2.0 && (double)this.mrSpeed >= 2.0 && (double)this.mrStartAudioDebounce < 0.0)
        {
            this.mLiftEntity.GetComponent<AudioSource>().Play();
            this.mrStartAudioDebounce = 1f;
        }
        if (this.mbStopSFXQueued && (double)this.mrSpeed > -1.5 && (double)this.mrSpeed < 1.5)
        {
            this.mArriveAudio.Play();
            this.mbStopSFXQueued = false;
        }
        if ((double)this.mrSpeed < -4.0 || (double)this.mrSpeed > 4.0)
            this.mbStopSFXQueued = true;
        if ((double)this.mrCurrentGas < (double)this.mrMaxGas)
        {
            this.mrCurrentGas += this.mrBaseGasRegen * Time.deltaTime;
            this.MarkDirtyDelayed();
        }
        long x;
        long y;
        long z;
        WorldScript.instance.mPlayerFrustrum.GetCoordsFromUnityWithRoundingFix(point, out x, out y, out z);

        //Collider velocities
        float vx = 0, vy = 0, vz = 0;
        //Collider offsets
        float ox = 0.0f, oy = 0.0f, oz = 0.0f;
        //Base and piston positions
        long xb = x - 1L, yb = y, zb = z - 1L, xp = x, yp = y, zp = z;
        //Piston sizes
        float xps = 1f, yps = 1f, zps = 1f;
        //Collider offsets
        float indexoff = this.mrCurrentExtend - Mathf.Floor(this.mrCurrentExtend);
        long CEoff = (long)(int)Mathf.Floor(this.mrCurrentExtend);

        if (this.mbPlayerIsNearby)
        {
            //Collider management
            switch ((int)mFlags & 63)
            {
                case 1:
                    yb = yb + CEoff;
                    oy = indexoff;
                    vy = vo;
                    yps = this.mrCurrentExtend;
                    break;
                case 2:
                    yb = yb - CEoff;
                    oy = -indexoff;
                    vy = -vo;
                    yp = yp - (long)this.mrCurrentExtend;
                    yps = this.mrCurrentExtend;
                    break;
                case 4:
                    zb = zb + CEoff;
                    oz = indexoff;
                    vz = vo;
                    zps = this.mrCurrentExtend;
                    break;
                case 8:
                    zb = zb - CEoff;
                    oz = -indexoff;
                    vz = -vo;
                    zp = zp - (long)this.mrCurrentExtend;
                    zps = this.mrCurrentExtend;
                    break;
                case 16:
                    xb = xb + CEoff;
                    ox = indexoff;
                    vx = vo;
                    xps = this.mrCurrentExtend;
                    break;
                case 32:
                    xb = xb - CEoff;
                    ox = -indexoff;
                    vx = -vo;
                    xp = xp - (long)this.mrCurrentExtend;
                    xps = this.mrCurrentExtend;
                    break;
                default:
                    yb = yb + CEoff;
                    oy = indexoff;
                    vy = vo;
                    yps = this.mrCurrentExtend;
                    break;
            }
            if (this.mnColliderRef == -1)
                this.mnColliderRef = WorldScript.instance.localPlayerInstance.RegisterCollider(xb, yb, zb, ox, oy, oz, 3f, 1f, 3f, vx, vy, vz);
            else
                WorldScript.instance.localPlayerInstance.UpdateCollider(this.mnColliderRef, xb, yb, zb, ox, oy, oz, 3f, 1f, 3f, vx, vy, vz);
            if (this.mnColliderPistonRef == -1)
                this.mnColliderPistonRef = WorldScript.instance.localPlayerInstance.RegisterCollider(xp, yp, zp, 0.0f, 0.0f, 0.0f, xps, yps, zps, 0.0f, 0.0f, 0.0f);
            else
                WorldScript.instance.localPlayerInstance.UpdateCollider(this.mnColliderPistonRef, xp, yp, zp, 0.0f, 0.0f, 0.0f, xps, yps, zps, 0.0f, 0.0f, 0.0f);
        }
        else
            this.UnregisterColliders();
        bool flag2 = false;

        //Detection of transporting player
        float relposy=100, relposx=100, relposz=100;
        switch ((int)mFlags & 63)
        {
            case 1:
                relposy = (float)((double)point.y + (double)this.mrCurrentExtend + 0.5);
                relposx = WorldScript.instance.localPlayerInstance.mPosition.x - point.x;
                relposz = WorldScript.instance.localPlayerInstance.mPosition.z - point.z;
                break;
            case 2:
                relposy = (float)((double)point.y - (double)this.mrCurrentExtend + 0.5);
                relposx = WorldScript.instance.localPlayerInstance.mPosition.x - point.x;
                relposz = WorldScript.instance.localPlayerInstance.mPosition.z - point.z;
                break;
            case 4:
                relposy = (float)((double)point.y + 0.5);
                relposx = WorldScript.instance.localPlayerInstance.mPosition.x - point.x;
                relposz = WorldScript.instance.localPlayerInstance.mPosition.z - point.z - this.mrCurrentExtend;
                break;
            case 8:
                relposy = (float)((double)point.y + 0.5);
                relposx = WorldScript.instance.localPlayerInstance.mPosition.x - point.x;
                relposz = WorldScript.instance.localPlayerInstance.mPosition.z - point.z + this.mrCurrentExtend;
                break;
            case 16:
                relposy = (float)((double)point.y + 0.5);
                relposx = WorldScript.instance.localPlayerInstance.mPosition.x - point.x - this.mrCurrentExtend;
                relposz = WorldScript.instance.localPlayerInstance.mPosition.z - point.z;
                break;
            case 32:
                relposy = (float)((double)point.y + 0.5);
                relposx = WorldScript.instance.localPlayerInstance.mPosition.x - point.x + this.mrCurrentExtend;
                relposz = WorldScript.instance.localPlayerInstance.mPosition.z - point.z;
                break;
            default:
                relposy = (float)((double)point.y + (double)this.mrCurrentExtend + 0.5);
                relposx = WorldScript.instance.localPlayerInstance.mPosition.x - point.x;
                relposz = WorldScript.instance.localPlayerInstance.mPosition.z - point.z;
                break;
        }

        if ((double)lrPlayerFeetY >= (double)relposy - 0.5 && (double)lrPlayerFeetY < (double)relposy + 1.0)
        {
            
            if ((double)relposx > -(double)this.mrOnLiftDist && (double)relposx < (double)this.mrOnLiftDist && ((double)relposz > -(double)this.mrOnLiftDist && (double)relposz < (double)this.mrOnLiftDist))
            {
                flag2 = true;
                if (this.meState != AdvancedLift.eLiftState.TransportingPlayer)
                {
                    this.meState = AdvancedLift.eLiftState.TransportingPlayer;
                    if ((double)this.mrCurrentExtend < (double)this.mnSafeMinExtend + 2.0)
                        this.SetDestination((float)this.mnTravelDist);
                    else if ((double)this.mrCurrentExtend > (double)this.mnSafeMaxExtend - 2.0)
                    {
                        this.SetDestination(0.0f);
                    }
                    else
                    { 
                        Vector3 basedirection = new Vector3 (this.mLiftEntity.mStartPos.x - lrPlayerFeetX, this.mLiftEntity.mStartPos.y - lrPlayerFeetY, this.mLiftEntity.mStartPos.z - lrPlayerFeetZ);
                        float num7 = Vector3.Dot(Vector3.Normalize(basedirection), Vector3.Normalize(WorldScript.instance.localPlayerInstance.mPlayer.mForward));
                        if ((double)num7 < -0.2)
                            this.SetDestination((float)this.mnTravelDist);
                        else if ((double)num7 > 0.2)
                            this.SetDestination(0.0f);
                        else if (this.meCurrentDirection == AdvancedLift.eLiftDirection.Upwards)
                            this.SetDestination((float)this.mnTravelDist);
                        else
                            this.SetDestination(0.0f);
                    }
                }
            }
        }
        if (!flag2)
        {
            bool flag3 = false;
            double playerfeet;
            double lowerbound = 0, upperbound = 0;
            float num5 = 0, num6 = 0;
            float playerpos = 0.0f;

            //Call lift to player
            switch ((int)mFlags & 63)
            {
                case 1:
                    playerfeet = (double)lrPlayerFeetY;
                    lowerbound = (double)point.y;
                    upperbound = (double)point.y + (double)this.mnTravelDist + 1.0;
                    num5 = WorldScript.instance.localPlayerInstance.mPosition.x - point.x;
                    num6 = WorldScript.instance.localPlayerInstance.mPosition.z - point.z;
                    playerpos = ((float)(playerfeet - (double)point.y - 0.550000011920929));
                    break;
                case 2:
                    playerfeet = (double)lrPlayerFeetY;
                    lowerbound = (double)point.y - (double)this.mnTravelDist - 1.0;
                    upperbound = (double)point.y;
                    num5 = WorldScript.instance.localPlayerInstance.mPosition.x - point.x;
                    num6 = WorldScript.instance.localPlayerInstance.mPosition.z - point.z;
                    playerpos = (float)(-playerfeet + (double)point.y + 0.550000011920929);
                    break;
                case 4:
                    playerfeet = (double)lrPlayerFeetZ;
                    lowerbound = (double)point.z;
                    upperbound = (double)point.z + (double)this.mnTravelDist + 1.0;
                    num5 = WorldScript.instance.localPlayerInstance.mPosition.x - point.x;
                    num6 = WorldScript.instance.localPlayerInstance.mPosition.y - point.y;
                    playerpos = ((float)(playerfeet - (double)point.z + 1));
                    break;
                case 8:
                    playerfeet = (double)lrPlayerFeetZ;
                    lowerbound = (double)point.z - (double)this.mnTravelDist - 1.0;
                    upperbound = (double)point.z;
                    num5 = WorldScript.instance.localPlayerInstance.mPosition.x - point.x;
                    num6 = WorldScript.instance.localPlayerInstance.mPosition.y - point.y;
                    playerpos = (float)(-playerfeet + (double)point.z + 1);
                    break;
                case 16:
                    playerfeet = (double)lrPlayerFeetX;
                    lowerbound = (double)point.x;
                    upperbound = (double)point.x + (double)this.mnTravelDist + 1.0;
                    num5 = WorldScript.instance.localPlayerInstance.mPosition.z - point.z;
                    num6 = WorldScript.instance.localPlayerInstance.mPosition.y - point.y;
                    playerpos = ((float)(playerfeet - (double)point.x + 1));
                    break;
                case 32:
                    playerfeet = (double)lrPlayerFeetX;
                    lowerbound = (double)point.x - (double)this.mnTravelDist - 1.0;
                    upperbound = (double)point.x;
                    num5 = WorldScript.instance.localPlayerInstance.mPosition.z - point.z;
                    num6 = WorldScript.instance.localPlayerInstance.mPosition.y - point.y;
                    playerpos = (float)(-playerfeet + (double)point.x + 1);
                    break;
                default:
                    playerfeet = (double)lrPlayerFeetY;
                    lowerbound = (double)point.y;
                    upperbound = (double)point.y + (double)this.mnTravelDist + 1.0;
                    num5 = WorldScript.instance.localPlayerInstance.mPosition.x - point.x;
                    num6 = WorldScript.instance.localPlayerInstance.mPosition.z - point.z;
                    break;
            }

            if (playerfeet> lowerbound && playerfeet < upperbound)
            {
                if ((double)num5 > -(double)this.mrCallLiftDist && (double)num5 < (double)this.mrCallLiftDist && ((double)num6 > -(double)this.mrCallLiftDist && (double)num6 < (double)this.mrCallLiftDist) && ((double)num5 < -(double)this.mrOnLiftDist || (double)num5 > (double)this.mrOnLiftDist || ((double)num6 < -(double)this.mrOnLiftDist || (double)num6 > (double)this.mrOnLiftDist)))
                {
                    this.meState = AdvancedLift.eLiftState.MovingToPlayer;
                    this.SetDestination(playerpos);
                    flag3 = true;
                }
            }
            if (playerfeet > lowerbound - (double)this.mrNearbyBelow && playerfeet < upperbound + (double)this.mrNearbyAbove)
            {
                this.mbPlayerIsNearby = (double)num5 > -(double)this.mrNearbyHorizontal && (double)num5 < (double)this.mrNearbyHorizontal && ((double)num6 > -(double)this.mrNearbyHorizontal && (double)num6 < (double)this.mrNearbyHorizontal);
            }
            else
                this.mbPlayerIsNearby = false;

            if (!flag3 && this.meState != AdvancedLift.eLiftState.Idle)
                this.meState = AdvancedLift.eLiftState.Idle;
        }
        else
            this.mbPlayerIsNearby = true;
        if (this.meState != AdvancedLift.eLiftState.TransportingPlayer)
            return;
        string lStr = "Extend : " + this.mrCurrentExtend.ToString("F0") + "/" + this.mnSafeMaxExtend.ToString("F0") + "\nPressure : " + this.mrCurrentGas.ToString("F2") + "psi\nCurrent Speed : " + this.mrSpeed.ToString("F2") + "mps";
        if (!this.mbHasPoweredCompressor)
            lStr = string.Concat(new object[4]
            {
        (object) lStr,
        (object) "\nPowered compressor required beyond ",
        (object) this.mnMaxExtendWithoutCompressor,
        (object) "m"
            });
        if (this.mbManuallyControlled)
            lStr = lStr + "\n(Home) Extend Lift." + "\n(End) Retract Lift." + "\n(Del) Stop Lift.";
        UIManager.instance.SetInfoText(lStr, 0.65f, false);
    }

    private void HandleManualLiftMovement(float lrPlayerFeetY, float lrPlayerFeetX, float lrPlayerFeetZ, float currentMaxSpeed)
    {
        Vector3 point = this.mLiftEntity.mStartPos;
        switch((int)mFlags & 63)
                {
                    case 1:
                        this.SetDestination((float)((double)lrPlayerFeetY - (double)point.y - 0.550000011920929));
                        break;
                    case 2:
                        this.SetDestination((float)(-(double)lrPlayerFeetY + (double)point.y + 0.550000011920929));
                        break;
                    case 4:
                        this.SetDestination((float)((double)lrPlayerFeetZ - (double)point.z));
                        break;
                    case 8:
                        this.SetDestination((float)(-(double)lrPlayerFeetZ + (double)point.z));
                        break;
                    case 16:
                        this.SetDestination((float)((double)lrPlayerFeetX - (double)point.x));
                        break;
                    case 32:
                        this.SetDestination((float)(-(double)lrPlayerFeetX + (double)point.x));
                        break;
                    default:
                        this.SetDestination((float)((double)lrPlayerFeetY - (double)point.y - 0.550000011920929));
                        break;
                }
        if (Input.GetKey(KeyCode.Home))
        {
            if (this.meCurrentDirection == AdvancedLift.eLiftDirection.Downwards)
            {
                switch ((int)mFlags & 63)
                {
                    case 1: 
                        break;
                    case 2:
                        this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate;
                        break;
                    case 4:
                    case 8:
                    case 16:
                    case 32:
                        this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate * 0.5f;
                        break;
                    default:
                        break;
                }
                this.mrSpeed -= Time.deltaTime * this.mrManualAcceleration;
                if ((double)this.mrSpeed <= 0.0)
                {
                    this.mrSpeed = 0.0f;
                    this.meCurrentDirection = AdvancedLift.eLiftDirection.Upwards;
                }
            }
            else
            {
                switch ((int)mFlags & 63)
                {
                    case 1:
                        this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate;
                        break;
                    case 2:
                        break;
                    case 4:
                    case 8:
                    case 16:
                    case 32:
                        this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate * 0.5f;
                        break;
                    default:
                        break;
                }
                if ((double)this.mrSpeed < (double)currentMaxSpeed)
                {
                    this.mrSpeed += Time.deltaTime * this.mrManualAcceleration;
                }
                else
                {
                    this.mrSpeed -= Time.deltaTime * this.mrManualAcceleration;
                    if ((double)this.mrSpeed < (double)currentMaxSpeed)
                        this.mrSpeed = currentMaxSpeed;
                }
            }
        }
        if (Input.GetKey(KeyCode.End))
            if (this.meCurrentDirection == AdvancedLift.eLiftDirection.Upwards)
            {
                switch ((int)mFlags & 63)
                {
                    case 1:
                        this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate;
                        break;
                    case 2:
                        break;
                    case 4:
                    case 8:
                    case 16:
                    case 32:
                        this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate * 0.5f;
                        break;
                    default:
                        break;
                }
                this.mrSpeed -= Time.deltaTime * this.mrManualAcceleration;
                if ((double)this.mrSpeed > 0.0)
                    return;
                this.mrSpeed = 0.0f;
                this.meCurrentDirection = AdvancedLift.eLiftDirection.Downwards;
            }
            else
            {
                switch ((int)mFlags & 63)
                {
                    case 1:
                        break;
                    case 2:
                        this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate;
                        break;
                    case 4:
                    case 8:
                    case 16:
                    case 32:
                        this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate * 0.5f;
                        break;
                    default:
                        break;
                }
                if ((double)this.mrSpeed >= (double)this.mrMaxSpeed)
                    return;
                this.mrSpeed += Time.deltaTime * this.mrManualAcceleration;
            }
        if (Input.GetKey(KeyCode.Delete) && this.mrSpeed != 0.0f)
        {
            switch ((int)mFlags & 63)
            {
                case 1:
                    if (this.meCurrentDirection == AdvancedLift.eLiftDirection.Upwards)
                        this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate;
                    break;
                case 2:
                    if (this.meCurrentDirection == AdvancedLift.eLiftDirection.Downwards)
                        this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate;
                    break;
                case 4:
                case 8:
                case 16:
                case 32:
                    this.mrCurrentGas -= Time.deltaTime * this.mrGasUsageRate * 0.5f;
                    break;
                default:
                    break;
            }
            this.mrSpeed -= Time.deltaTime * this.mrManualAcceleration;
            if ((double)this.mrSpeed > 0.0)
                return;
            this.mrSpeed = 0.0f;
        }
    }

    public void SetDestination(float liftDestination)
    {
        //Debug.Log("liftdestination: " + liftDestination + "Safemin: " + mnSafeMinExtend + "Safemax: " + mnSafeMaxExtend);
        this.mrLiftDestination = liftDestination;
        if ((double)this.mrLiftDestination < 0.0)
            this.mrLiftDestination = 0.0f;
        if ((double)this.mrLiftDestination > (double)this.mnTravelDist)
            this.mrLiftDestination = (float)this.mnTravelDist;
        this.mrDesiredLiftDestination = this.mrLiftDestination;
        if ((double)this.mrLiftDestination < (double)this.mnSafeMinExtend)
            this.mrLiftDestination = (float)this.mnSafeMinExtend;
        if ((double)this.mrLiftDestination <= (double)this.mnSafeMaxExtend)
            return;
        this.mrLiftDestination = (float)this.mnSafeMaxExtend;
    }

    public override HoloMachineEntity CreateHolobaseEntity(Holobase holobase)
    {
        HolobaseEntityCreationParameters parameters = new HolobaseEntityCreationParameters((SegmentEntity)this);
        HolobaseVisualisationParameters visualisationParameters1 = parameters.AddVisualisation(holobase.PowerStorage);
        visualisationParameters1.Scale = new Vector3(1f, 1f, 1f);
        visualisationParameters1.Color = new Color(1f, 0.7f, 0.1f);
        HolobaseVisualisationParameters visualisationParameters2 = parameters.AddVisualisation("PassengerLift", holobase.mPreviewCube);
        visualisationParameters2.Scale = new Vector3(3f, 0.5f, 3f);
        visualisationParameters2.Color = new Color(0.1f, 0.7f, 1.1f);
        parameters.RequiresUpdates = true;
        this.firstrunholobase = true;
        return holobase.CreateHolobaseEntity(parameters);
    }

    public override void HolobaseUpdate(Holobase holobase, HoloMachineEntity holoMachineEntity)
    {
        GameObject gameObject = holoMachineEntity.VisualisationObjects[1];
        Vector3 vector3 = Vector3.zero;
        switch ((int)mFlags & 63)
        {
            case 1:
                vector3 = holoMachineEntity.VisualisationObjects[0].transform.localPosition + new Vector3(0.0f, this.mrCurrentExtend + 1.25f, 0.0f);
                break;
            case 2:
                vector3 = holoMachineEntity.VisualisationObjects[0].transform.localPosition + new Vector3(0.0f, -this.mrCurrentExtend - 0.75f, 0.0f);
                break;
            case 4:
                if (this.firstrunholobase)
                    gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                vector3 = holoMachineEntity.VisualisationObjects[0].transform.localPosition + new Vector3(0.0f, 0.25f, this.mrCurrentExtend + 1f);
                break;
            case 8:
                if (this.firstrunholobase)
                    gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                vector3 = holoMachineEntity.VisualisationObjects[0].transform.localPosition + new Vector3(0.0f, 0.25f, -this.mrCurrentExtend - 1f);
                break;
            case 16:
                if (this.firstrunholobase)
                    gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                vector3 = holoMachineEntity.VisualisationObjects[0].transform.localPosition + new Vector3(this.mrCurrentExtend + 1f, 0.25f, 0.0f);
                break;
            case 32:
                if (this.firstrunholobase)
                    gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                vector3 = holoMachineEntity.VisualisationObjects[0].transform.localPosition + new Vector3(-this.mrCurrentExtend - 1f, 0.25f, 0.0f);
                break;
            default:
                break;
        }
        if ((double)Holobase.mrBaseActiveTime < 1.0)
            gameObject.transform.localPosition = vector3;
        gameObject.transform.localPosition += (vector3 - gameObject.transform.localPosition) * Time.deltaTime * 15f;
        this.firstrunholobase = false;
        if (this.mbDelete)
        {
            gameObject.SetActive(false);
            holoMachineEntity.VisualisationObjects[0].SetActive(false);
        }
    }

    public void HasPoweredCompressor()
    {
        this.mrTimeSinceLastPoweredCompressorNotification = 0.0f;
    }

    public override bool ShouldNetworkUpdate()
    {
        return true;
    }

    private enum eLiftDirection
    {
        Upwards,
        Downwards,
    }

    private enum eLiftState
    {
        Idle,
        MovingToPlayer,
        TransportingPlayer,
    }
}
