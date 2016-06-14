using System.IO;
using UnityEngine;

public class AdvLiftCompressor : MachineEntity, PowerConsumerInterface
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
    public bool mbCompressorActive;

    

    public AdvLiftCompressor(Segment segment, long x, long y, long z, ushort cube, byte flags, ushort lValue)
      : base(eSegmentEntity.Mod, SpawnableObjectEnum.Lift_Compressor, x, y, z, cube, flags, lValue, Vector3.zero, segment)
    {
        this.mbNeedsLowFrequencyUpdate = true;
    }

    public override string GetPopupText()
    {
        AdvLiftCompressor compressorEntity = (AdvLiftCompressor)WorldScript.instance.localPlayerInstance.mPlayerBlockPicker.selectedEntity;
        if (compressorEntity != null)
        {
            string str = "Lift Compression Module";
            if (compressorEntity.mbAttachedToLift)
                str += "\nAttached to Lift";
            
            return !compressorEntity.mbCompressorActive ? str + "\nCompressor Idle." : str + "\nCompressor Active!" + "\nPower " + compressorEntity.mrCurrentPower.ToString("F0") + "/" + compressorEntity.mrMaxPower.ToString("F0");
        }
        else
            return (string)null;
    }

    public override void LowFrequencyUpdate()
    {
       
        ++this.mnLFUpdates;
        if (!this.mbAttachedToLift)
        {
            this.AttemptToFindLift();
            if (this.mnLFUpdates < 12 || this.mbAttachedToLift || !WorldScript.mbIsServer)
                return;
            WorldScript.instance.BuildFromEntity(this.mSegment, this.mnX, this.mnY, this.mnZ, (ushort)1, (ushort)0);
            ItemManager.DropNewCubeStack(ModManager.mModMappings.CubesByKey["steveman0.AdvLiftCompressor"].CubeType, this.mValue, 1, this.mnX, this.mnY, this.mnZ, Vector3.zero);
        }
        else
        {
            if (this.mnLFUpdates % 10 == 0)
            {
                Segment segment = this.AttemptGetSegment(this.mLiftX, this.mLiftY, this.mLiftZ);
                if (segment == null)
                    return;
                if ((int)segment.GetCube(this.mLiftX, this.mLiftY, this.mLiftZ) != ModManager.mModMappings.CubesByKey["steveman0.AdvancedLift"].CubeType)
                {
                    if (!WorldScript.mbIsServer)
                        return;
                    WorldScript.instance.BuildFromEntity(this.mSegment, this.mnX, this.mnY, this.mnZ, (ushort)1, (ushort)0);
                    ItemManager.DropNewCubeStack(ModManager.mModMappings.CubesByKey["steveman0.AdvLiftCompressor"].CubeType, this.mValue, 1, this.mnX, this.mnY, this.mnZ, Vector3.zero);
                    return;
                }
                AdvancedLift t1LiftEntity = (AdvancedLift)segment.FetchEntity(eSegmentEntity.T1_Lift, this.mLiftX, this.mLiftY, this.mLiftZ);
                if (t1LiftEntity != null && this.mLift != t1LiftEntity)
                {
                    this.mLift = (AdvancedLift)null;
                    this.mLift = t1LiftEntity;
                    Debug.LogWarning((object)"Lift Compressor has latched onto different lift! (yay)");
                }
            }
            if (this.mLift == null || (double)this.mrCurrentPower <= (double)LowFrequencyThread.mrPreviousUpdateTimeStep)
                return;
            this.mLift.HasPoweredCompressor();
            if ((double)this.mLift.mrStoredGas < (double)this.mLift.mrMaxStoredGas)
            {
                this.mrCurrentPower -= LowFrequencyThread.mrPreviousUpdateTimeStep;
                this.mLift.mrStoredGas += LowFrequencyThread.mrPreviousUpdateTimeStep * 0.15f;
                this.mbCompressorActive = true;
            }
            else
                this.mbCompressorActive = false;
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

    public float GetRemainingPowerCapacity()
    {
        return this.mrMaxPower - this.mrCurrentPower;
    }

    public float GetMaximumDeliveryRate()
    {
        return 5f;
    }

    public float GetMaxPower()
    {
        return this.mrMaxPower;
    }

    public bool DeliverPower(float amount)
    {
        if ((double)amount > (double)this.GetRemainingPowerCapacity())
            return false;
        this.mrCurrentPower += amount;
        return true;
    }

    public bool WantsPowerFromEntity(SegmentEntity entity)
    {
        return true;
    }
}