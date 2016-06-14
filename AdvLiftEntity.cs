using System;
using UnityEngine;

public class AdvLiftEntity : MonoBehaviour
    {
        public Vector3 mStartPos;

        public GameObject PistonObject;

        public AdvancedLift mMachineEntity;

        public bool mbFirstRun = true;

        private void Start()
        {
            this.mStartPos = base.transform.position;
        }

        public void DroppingEntity()
        {
            this.mMachineEntity = null;
        }

        private void FixedUpdate()
        {
            AdvancedLift AdvancedLift = this.mMachineEntity;
            if (object.ReferenceEquals(AdvancedLift, null))
            {
                return;
            }
            AdvancedLift.HandleFixedUpdate();
        }

    public void UpdatePositions(float currentExtend, byte flags)
    {
        //Switch to account for all 6 possible lift orientations
        switch ((int)flags & 63)
        {
            //normal upright
            case 1:
                base.transform.position = this.mStartPos + new Vector3(0f, currentExtend, 0f);
                this.PistonObject.transform.position = this.mStartPos + new Vector3(0f, currentExtend / 2f - 0.5f, 0f);
                this.PistonObject.transform.localScale = new Vector3(1f, currentExtend, 1f);
                this.mbFirstRun = false;
                break;
            //upside down (suspended)
            case 2:
                if (mbFirstRun)
                    base.transform.RotateAround(transform.position, transform.right, 180f);
                this.mbFirstRun = false;
                base.transform.position = this.mStartPos + new Vector3(0f, -currentExtend, 0f);
                this.PistonObject.transform.position = this.mStartPos + new Vector3(0f, -currentExtend / 2f + 0.5f, 0f);
                this.PistonObject.transform.localScale = new Vector3(1f, -currentExtend, 1f);
                break;
            //+z
            case 4:
                if (mbFirstRun)
                {
                    base.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    this.PistonObject.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                    //this.PistonObject.transform.position = this.mStartPos + new Vector3(0f, 1f, currentExtend / 2f - 0.5f);
                }
                base.transform.position = this.mStartPos + new Vector3(0f, 0f, currentExtend);
                this.PistonObject.transform.position = this.mStartPos + new Vector3(0f, 0f, currentExtend / 2f - 0.5f);
                this.PistonObject.transform.localScale = new Vector3(1f, currentExtend, 1f);
                this.mbFirstRun = false;
                break;
            //-z
            case 8:
                if (mbFirstRun)
                {
                    base.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    this.PistonObject.transform.rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
                    //this.PistonObject.transform.position = this.mStartPos + new Vector3(0f, 1f, -currentExtend / 2f + 0.5f);
                }
                base.transform.position = this.mStartPos + new Vector3(0f, 0f, -currentExtend);
                this.PistonObject.transform.position = this.mStartPos + new Vector3(0f, 0f, -currentExtend / 2f + 0.5f);
                this.PistonObject.transform.localScale = new Vector3(1f, -currentExtend, 1f);
                this.mbFirstRun = false;
                break;
            //+x
            case 16:
                if (mbFirstRun)
                {
                    base.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    this.PistonObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                    //this.PistonObject.transform.position = this.mStartPos + new Vector3(currentExtend / 2f - 0.5f, 1f, 0f);
                }
                base.transform.position = this.mStartPos + new Vector3(currentExtend, 0f, 0f);
                this.PistonObject.transform.position = this.mStartPos + new Vector3(currentExtend / 2f - 0.5f, 0f, 0f);
                this.PistonObject.transform.localScale = new Vector3(1f, currentExtend, 1f);
                this.mbFirstRun = false;
                break;
            //-x
            case 32:
                if (mbFirstRun)
                {
                    base.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    this.PistonObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
                    //this.PistonObject.transform.position = this.mStartPos + new Vector3(-currentExtend / 2f + 0.5f, 1f, 0f);
                }
                base.transform.position = this.mStartPos + new Vector3(-currentExtend, 0f, 0f);
                this.PistonObject.transform.position = this.mStartPos + new Vector3(-currentExtend / 2f + 0.5f, 0f, 0f);
                this.PistonObject.transform.localScale = new Vector3(1f, -currentExtend, 1f);
                this.mbFirstRun = false;
                break;
            default:
                base.transform.position = this.mStartPos + new Vector3(0f, currentExtend, 0f);
                this.PistonObject.transform.position = this.mStartPos + new Vector3(0f, currentExtend / 2f - 0.5f, 0f);
                this.PistonObject.transform.localScale = new Vector3(1f, currentExtend, 1f);
                this.mbFirstRun = false;
                break;
        }
    }  

        public void ExtentsUpdated()
        {
        }
    }

