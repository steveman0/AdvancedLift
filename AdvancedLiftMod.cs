using UnityEngine;

public class AdvancedLiftMod : FortressCraftMod
{

    public override ModRegistrationData Register()
    {
        ModRegistrationData modRegistrationData = new ModRegistrationData();
        modRegistrationData.RegisterEntityHandler("steveman0.AdvancedLift");
        modRegistrationData.RegisterEntityHandler("steveman0.AdvLiftManualControl");
        modRegistrationData.RegisterEntityHandler("steveman0.AdvLiftCompressor");

        Debug.Log("Advanced Lift Mod V1.1 registered");

        //Network interface registration
        UIManager.NetworkCommandFunctions.Add("Adv_Lift", new UIManager.HandleNetworkCommand(AdvLiftWindow.HandleNetworkCommand));

        return modRegistrationData;
    }

    public override ModCreateSegmentEntityResults CreateSegmentEntity(ModCreateSegmentEntityParameters parameters)
    {
        ModCreateSegmentEntityResults result = new ModCreateSegmentEntityResults();

        foreach (ModCubeMap cubeMap in ModManager.mModMappings.CubeTypes)
        {
            if (cubeMap.CubeType == parameters.Cube)
            {
                if (cubeMap.Key.Equals("steveman0.AdvancedLift"))
                    result.Entity = new AdvancedLift(parameters.Segment, parameters.X, parameters.Y, parameters.Z, parameters.Cube, parameters.Flags, parameters.Value);
                if (cubeMap.Key.Equals("steveman0.AdvLiftCompressor"))
                    result.Entity = new AdvLiftCompressor(parameters.Segment, parameters.X, parameters.Y, parameters.Z, parameters.Cube, parameters.Flags, parameters.Value);
                if (cubeMap.Key.Equals("steveman0.AdvLiftManualControl"))
                    result.Entity = new AdvLiftManualControl(parameters.Segment, parameters.X, parameters.Y, parameters.Z, parameters.Cube, parameters.Flags, parameters.Value);
            }
        }
        return result;
    }

    //public static ushort GetCubeFromKey(string key)
    //{
    //    ushort result;
    //    foreach (ModCubeMap current in ModManager.mModMappings.CubeTypes)
    //    {
    //        if (current.Key == key)
    //        {
    //            result = current.CubeType;
    //            return result;
    //        } 
    //    }
    //    result = 0;
    //    return result;
        
    //}
}

