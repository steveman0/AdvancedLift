public abstract class AdvLiftWindow
{
    public const string InterfaceName = "Adv_Lift";
    public const string InterfaceSetRange = "SetRange";

    public static void AlterExtendRange(Player player, AdvancedLift lift, bool lbFineControl)
    {
        lift.AlterExtendRange(lbFineControl);
        if (WorldScript.mbIsServer)
            return;
        NetworkManager.instance.SendInterfaceCommand("Adv_Lift", "SetRange", lift.mnExtendRange.ToString(), (ItemBase)null, (SegmentEntity)lift, 0.0f);
    }

    public static void SetRange(Player player, AdvancedLift lift, int newRange)
    {
        lift.SetRange(newRange);
        if (WorldScript.mbIsServer)
            return;
        NetworkManager.instance.SendInterfaceCommand("Adv_Lift", "SetRange", lift.mnExtendRange.ToString(), (ItemBase)null, (SegmentEntity)lift, 0.0f);
    }

    public static NetworkInterfaceResponse HandleNetworkCommand(Player player, NetworkInterfaceCommand nic)
    {
        AdvancedLift t1LiftEntity = nic.target as AdvancedLift;
        string key = nic.command;
        if (key != null)
        {
            if (key == "SetRange")
            {
                int range;
                if (int.TryParse(nic.payload ?? "8", out range))
                    t1LiftEntity.SetRange(range);
            }
        }
        return new NetworkInterfaceResponse()
        {
            entity = (SegmentEntity)t1LiftEntity
        };
    }
}