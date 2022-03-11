﻿using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VisualAdjustments2.Infrastructure
{
    public class CharacterSettings
    {
        public EESettings EeSettings = new EESettings();
        public class EESettings
        {
            public List<EE_Applier> EEs = new List<EE_Applier>();
        }
        public Dictionary<ItemsFilter.ItemType, bool> HideEquipmentDict = SetupDict();
        public static Dictionary<ItemsFilter.ItemType, bool> SetupDict()
        {
            var newdict = new Dictionary<ItemsFilter.ItemType, bool>();
            foreach(var type in Enum.GetValues(typeof(ItemsFilter.ItemType)))
            {
                newdict.Add((ItemsFilter.ItemType)type, false);
            }
            return newdict;
        }
    }
    public struct SerializableColor
    {
        public SerializableColor(Color col)
        {
            r = col.r;
            g = col.g;
            b = col.b;
        }

        public float r;
        public float g;
        public float b;
        public Color ToColor()
        {
            return new Color(r, g, b);
        }
    }
    public class EE_Applier
    {
        public enum ActionType
        {
            Add,
            Remove
        }
        public EquipmentEntity Load()
        {
            return ResourcesLibrary.TryGetResource<EquipmentEntity>(this.GUID);
        }
        public void Apply(Character character)
        {
            switch (this.actionType)
            {
                case ActionType.Add:
                    {
                        AddEE(character);
                        break;
                    }
                case ActionType.Remove:
                    {
                        RemoveEE(character);
                        break;
                    }
            }
        }
        private void RemoveEE(Character character)
        {
            var loadedEE = this.Load();
            if (character.EquipmentEntities.Any(b => b.name == loadedEE.name)) character.EquipmentEntities.Remove(loadedEE);
        }
        private void AddEE(Character character)
        {
            var loadedEE = this.Load();
            if (!character.EquipmentEntities.Contains(loadedEE)) character.EquipmentEntities.Add(loadedEE);
            Primary.Apply(loadedEE, character);
            Secondary.Apply(loadedEE, character);

        }
        public EE_Applier(string guid,ActionType actiontype)
        {
            GUID = guid;
            actionType = actiontype;
        }
        public ActionType actionType;
        public string GUID;
        public ColorInfo Primary = new ColorInfo(true);
        public ColorInfo Secondary = new ColorInfo(false);


        public class ColorInfo
        {
            public bool PrimOrSec;
            public bool CustomColor = false;
            public int Index;
            public ColorInfo(bool b)
            {
                PrimOrSec = b;
                Index = 1;
            }
            public SerializableColor CustomColorRGB;
            public void Apply(EquipmentEntity ee, Character character)
            {
                try
                {
                    if (CustomColor)
                    {
                        var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false)
                        {
                            filterMode = FilterMode.Bilinear
                        };
                        
                        var col = CustomColorRGB.ToColor();
                        tex.SetPixel(1, 1, col);
                        tex.Apply();
                        if (PrimOrSec)
                        {
                            ee.PrimaryColorsProfile.Ramps.Add(tex);
                            var index = ee.PrimaryColorsProfile.Ramps.IndexOf(tex);
                            character.SetPrimaryRampIndex(ee, index);
                        }
                        else
                        {
                            ee.SecondaryColorsProfile.Ramps.Add(tex);
                            var index = ee.SecondaryColorsProfile.Ramps.IndexOf(tex);
                            character.SetSecondaryRampIndex(ee, index);
                        }
                    }
                    else
                    {
                        if (PrimOrSec) character.SetPrimaryRampIndex(ee, Index);
                        else character.SetSecondaryRampIndex(ee, Index);
                    }
                    character.IsDirty = true;
                }
                catch(Exception e)
                {
                    Main.Logger.Error(e.ToString());
                }
            }
        }
    }
}