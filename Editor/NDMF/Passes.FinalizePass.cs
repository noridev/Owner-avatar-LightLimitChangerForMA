﻿using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace io.github.azukimochi
{
    partial class Passes
    {
        internal sealed class FinalizePass : LightLimitChangerBasePass<FinalizePass>
        {
            protected override void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache)
            {
                var obj = session.Settings.gameObject;
                var mergeAnimator = obj.GetOrAddComponent<ModularAvatarMergeAnimator>();
                var maParameters = obj.GetOrAddComponent<ModularAvatarParameters>();
                var menuInstaller = obj.GetOrAddComponent<ModularAvatarMenuInstaller>();

                mergeAnimator.animator = session.Controller;
                mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
                mergeAnimator.matchAvatarWriteDefaults = true;

                maParameters.parameters.AddRange(session.Controller.parameters.Select(x => new ParameterConfig()
                {
                    nameOrPrefix = x.name,
                    internalParameter = true,
                    syncType = x.type == AnimatorControllerParameterType.Float ? ParameterSyncType.Float : ParameterSyncType.Bool,
                    defaultValue = x.type == AnimatorControllerParameterType.Float ? x.defaultFloat : (x.defaultBool ? 1 : 0),
                    saved = session.Parameters.IsValueSave,
                    localOnly = x.name == ParameterName_Reset
                }));

                menuInstaller.menuToAppend = CreateMenu(session, cache);

                foreach(var component in context.AvatarRootObject.GetComponentsInChildren<LightLimitChangerSettings>(true))
                {
                    component.Destroy();
                }
            }


            private static VRCExpressionsMenu CreateMenu(Session session, LightLimitChangerObjectCache cache)
            {
                var mainMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>().AddTo(cache);
                VRCExpressionsMenu additionalMenu = null;
                mainMenu.name = "Main Menu";
                mainMenu.controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control
                    {
                        name = "Enable",
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("94e101a18f0647c448df7fc3193aa474")),
                        parameter = new VRCExpressionsMenu.Control.Parameter
                        {
                            name = ParameterName_Toggle,
                        },
                    },
                };

                foreach (ref readonly var control in session.Controls.AsSpan())
                {
                    if (!session.TargetControl.HasFlag(control.ControlType))
                        continue;

                    var menu = LightLimitControlType.AdditionalControls.HasFlag(control.ControlType) ? additionalMenu : mainMenu;


                    if (menu == null && session.Parameters.IsGroupingAdditionalControls)
                    {
                        menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>().AddTo(cache);

                        mainMenu.controls.Add(new VRCExpressionsMenu.Control
                        {
                            name = "Controls",
                            type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                            icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("75cdf4ab5baf72a4f8b6463d3720bd35")),
                            subMenu = menu,
                        });

                        additionalMenu = menu;
                    }

                    menu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = control.Name,
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new[]
                        {
                            new VRCExpressionsMenu.Control.Parameter
                            {
                                name = control.ParameterName
                            }
                        },
                    });
                }

                if (session.Parameters.AddResetButton)
                {
                    mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Reset",
                        type = VRCExpressionsMenu.Control.ControlType.Button,
                        icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("46b69c6755e703048845eef57e51a329")),
                        parameter = new VRCExpressionsMenu.Control.Parameter() { name = ParameterName_Reset }
                    });
                }

                var rootMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>().AddTo(cache);
                {
                    rootMenu.name = "Root Menu";
                    rootMenu.controls = new List<VRCExpressionsMenu.Control>
                    {
                        new VRCExpressionsMenu.Control
                        {
                            name = "Light Limit Changer",
                            icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("09c1f5650f9952f49a0fbb551e64dcad")),
                            type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                            subMenu = mainMenu,
                        },
                    };
                };

                return rootMenu;
            }

        }
    }
}
