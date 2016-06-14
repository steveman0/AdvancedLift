using System.IO;
using UnityEngine;

public class AdvLiftManualControl : MachineEntity
{
    public float mrMaxPower = 150f;
    public const float POWER_TRANSFER_RATE = 5f;
    public float mrCurrentPower;
    public bool mbAttachedToLift;
    private int mnLFUpdates;
    private AdvancedLift mLift;
    private long mLiftX;
    private long mLiftY;
    private long mLiftZ;

    public AdvLiftManualControl(Segment segment, long x, long y, long z, ushort cube, byte flags, ushort lValue)
      : base(eSegmentEntity.Mod, SpawnableObjectEnum.Lift_ManualControl, x, y, z, cube, flags, lValue, Vector3.zero, segment)
    {
        this.mbNeedsLowFrequencyUpdate = true;
    }

    public override string GetPopupText()
    {
        AdvLiftManualControl manualControlEntity = (AdvLiftManualControl)WorldScript.instance.localPlayerInstance.mPlayerBlockPicker.selectedEntity;
        if (manualControlEntity != null)
        {
            string lStr = "Lift Manual Override module";
            if (manualControlEntity.mbAttachedToLift)
                lStr += "\nAttached to Lift";
            return lStr;
        }
        else
            return (string)null;
    }

    public override void LowFrequencyUpdate()
    {
        ++this.mnLFUpdates;
        if (this.mnLFUpdates < 2)
            return;
        if (!this.mbAttachedToLift)
        {
            this.AttemptToFindLift();
            if (this.mnLFUpdates < 12 || !WorldScript.mbIsServer)
                return;
            WorldScript.instance.BuildFromEntity(this.mSegment, this.mnX, this.mnY, this.mnZ, (ushort)1, (ushort)0);
            ItemManager.DropNewCubeStack(ModManager.mModMappings.CubesByKey["steveman0.AdvLiftManualControl"].CubeType, this.mValue, 1, this.mnX, this.mnY, this.mnZ, Vector3.zero);
        }
        else
        {
            if (this.mnLFUpdates % 10 != 0)
                return;
            Segment segment = this.AttemptGetSegment(this.mLiftX, this.mLiftY, this.mLiftZ);
            if (segment == null || (int)segment.GetCube(this.mLiftX, this.mLiftY, this.mLiftZ) == ModManager.mModMappings.CubesByKey["steveman0.AdvancedLift"].CubeType || !WorldScript.mbIsServer)
                return;
            WorldScript.instance.BuildFromEntity(this.mSegment, this.mnX, this.mnY, this.mnZ, (ushort)1, (ushort)0);
            ItemManager.DropNewCubeStack(ModManager.mModMappings.CubesByKey["steveman0.AdvLiftManualControl"].CubeType, this.mValue, 1, this.mnX, this.mnY, this.mnZ, Vector3.zero);
        }
    }

    private void AttemptToFindLift()
    {
        long num1 = this.mnX;
        long num2 = this.mnY;
        long num3 = this.mnZ;
        int num4 = 0;
        int num5 = 0;
        int num6 = 0;
        if (this.mnLFUpdates % 6 == 0)
            --num4;
        if (this.mnLFUpdates % 6 == 1)
            ++num4;
        if (this.mnLFUpdates % 6 == 2)
            --num5;
        if (this.mnLFUpdates % 6 == 3)
            ++num5;
        if (this.mnLFUpdates % 6 == 4)
            --num6;
        if (this.mnLFUpdates % 6 == 5)
            ++num6;
        long x = num1 + (long)num4;
        long y = num2 + (long)num5;
        long z = num3 + (long)num6;
        Segment segment = this.AttemptGetSegment(x, y, z);
        if (segment == null)
        {
            this.mnLFUpdates = 0;
        }
        else
        {
       
            if ((int)segment.GetCube(x, y, z) != ModManager.mModMappings.CubesByKey["steveman0.AdvancedLift"].CubeType)
                return;
            AdvancedLift t1LiftEntity = (AdvancedLift)segment.FetchEntity(eSegmentEntity.T1_Lift, x, y, z);
            if (t1LiftEntity == null)
                return;
            this.mbAttachedToLift = true;
            this.mLift = t1LiftEntity;
            this.mLiftX = x;
            this.mLiftY = y;
            this.mLiftZ = z;
            Debug.LogWarning((object)"Lift control module activing manual mode");
            this.mLift.mbManualControlAttached = true;
            this.mLift.mbNeedsUnityUpdate = true;
        }
    }

    public override bool ShouldSave()
    {
        return false;
    }

    public override void Write(BinaryWriter writer)
    {
    }

    public override void Read(BinaryReader reader, int entityVersion)
    {
    }

    public override void OnDelete()
    {
        if (this.mLift != null)
        {
            this.mLift.mbManualControlAttached = false;
            this.mLift.mbNeedsUnityUpdate = true;
        }
        base.OnDelete();
    }
}