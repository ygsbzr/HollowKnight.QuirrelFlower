using Modding;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Vasi;
using System.Reflection;
using System.Collections;
using CustomKnight;
namespace QuirrelFlower
{
    public class QuirrelFlower:Mod,ILocalSettings<Setting>
    {
        public override string GetVersion()
        {
            return "1.1";
        }
        private Texture2D QuirrelTex;
        public static Setting LS = new();
        public void OnLoadLocal(Setting s) => LS = s;
        public Setting OnSaveLocal() => LS;
        public override List<(string,string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Town","_NPCs/Elderbug/Flower Give")
            };
        }
        private GameObject origflowergive;
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            origflowergive = preloadedObjects["Town"]["_NPCs/Elderbug/Flower Give"];
            QuirrelTex = LoadQuirrelTex();
            On.PlayMakerFSM.Start += EditFsm;
            ModHooks.LanguageGetHook += EditLang;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += EditQui;
            
        }

        private void EditQui(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            if (arg1.name == "Crossroads_50")
            {
                GameObject quirrel = UnityEngine.Object.FindObjectsOfType<GameObject>().FirstOrDefault(x=>x.name== "Quirrel Lakeside");
                if (LS.giveFlower)
                {
                    foreach (var fsm in quirrel.GetComponents<PlayMakerFSM>().Where(x => (x.FsmName != "Conversation Control" && x.FsmName != "npc_control")))
                    {
                        UnityEngine.Object.Destroy(fsm);
                    }
                    quirrel.SetActive(true);
                    if(CKInstall())
                    {
                        CKReplace();
                    }
                    else
                    {
                        quirrel.GetComponent<tk2dSprite>().GetCurrentSpriteDef().material.mainTexture = QuirrelTex;
                    }
                }
                GameObject.Find("quirrel_death_nail").SetActive(false);
            }
        }

        private void EditFsm(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            if (self.gameObject.name == "Quirrel Lakeside" && self.FsmName == "Conversation Control")
            {
                EditQuirrelCovFSM(self);
            }
            orig(self);
        }

        

        private string EditLang(string key, string sheetTitle, string orig)
        {
            if(sheetTitle==QuirrelLang.QuirrelSheet)
            {
               if(Language.Language.CurrentLanguage().ToString().ToLower().Equals("zh"))
                {
                    if (QuirrelLang.QLanguagesCN.Keys.Contains(key))
                    {
                        return QuirrelLang.QLanguagesCN[key];
                    }
                }
                else
                {
                    if (QuirrelLang.QLanguagesEN.Keys.Contains(key))
                    {
                        return QuirrelLang.QLanguagesEN[key];
                    }
                }
            }
            return orig;
        }

        private void EditQuirrelCovFSM(PlayMakerFSM qfsm)
        {
            qfsm.GetState("Talk Finish").InsertMethod(0,() =>
            {
                qfsm.gameObject.GetComponent<tk2dSpriteAnimator>().Play("Lake Talk End");
            });
            GameObject giveclone = UnityEngine.Object.Instantiate(origflowergive, qfsm.gameObject.transform);
            giveclone.SetActive(false);
            FsmState hasGiveFlower = qfsm.CreateState("Has Give Flower?");
            FsmState checkFlower = qfsm.CreateState("Check Flower");
            FsmState flowerOffer = qfsm.CopyState("Talk 2", "Flower Offer");
            FsmState flowerBoxDown = qfsm.CopyState("Box Down", "Flower Box Down");
            FsmState flowerBoxYN = qfsm.CopyState("Box Down", "Flower Box Up YN");
            FsmState sendFlowerText = qfsm.CopyState("Box Down", "Flower Text");
            FsmState declineBoxDown = qfsm.CopyState("Box Down","Decline");
            FsmState acceptBoxDown = qfsm.CopyState("Box Down","Accept");
            FsmState declineBoxup= qfsm.CopyState("Box Up","Box Up 2");
            FsmState acceptBoxUp = qfsm.CopyState("Box Up","Box Up 3");
            FsmState declineConvo = qfsm.CopyState("Talk 2", "Decline Flower");
            FsmState acceptConvo = qfsm.CopyState("Talk 2", "Accept Flower");
            FsmState repeatConvo= qfsm.CopyState("Talk 2", "Flower Already Given");
            FsmState quirrelTexChange = qfsm.CreateState("Change Tex");
            hasGiveFlower.AddAction(new Vasi.InvokeMethod(() =>
            {
                if(LS.giveFlower)
                {
                    qfsm.SendEvent("FLOWER GIVEN");
                }
                else
                {
                    qfsm.SendEvent("FLOWER CHECK");
                }
            }));
            qfsm.ChangeTransition("Box Up", "FINISHED", "Has Give Flower?");
            hasGiveFlower.Transitions = Array.Empty<FsmTransition>();
            hasGiveFlower.AddTransition("FLOWER GIVEN", "Flower Already Given");
            hasGiveFlower.AddTransition("FLOWER CHECK", "Check Flower");
            modifyconvostate(repeatConvo, QuirrelLang.RepeatKey);
            repeatConvo.Transitions = Array.Empty<FsmTransition>();
            repeatConvo.AddTransition("CONVO_FINISH", "Talk Finish");
            repeatConvo.RemoveAction(3);
            checkFlower.AddAction(new Vasi.InvokeMethod(() =>
            {
                if(PlayerData.instance.GetBool(nameof(PlayerData.hasXunFlower))&&!PlayerData.instance.GetBool(nameof(PlayerData.xunFlowerBroken)))
                {
                    qfsm.SendEvent("HAS FLOWER");
                }
                else
                {
                    qfsm.SendEvent("HASNT FLOWER");
                }
            }));
            checkFlower.Transitions=Array.Empty<FsmTransition>();
            checkFlower.AddTransition("HASNT FLOWER", "Talk");
            checkFlower.AddTransition("HAS FLOWER", "Flower Offer");
            flowerOffer.RemoveAction(3);
            modifyconvostate(flowerOffer, QuirrelLang.FlowerOfferKey);
            flowerOffer.Transitions=Array.Empty<FsmTransition>();
            flowerOffer.AddTransition("CONVO_FINISH", "Flower Box Down");
            flowerBoxDown.RemoveAction(1);
            flowerBoxDown.Transitions=Array.Empty<FsmTransition>();
            flowerBoxDown.AddTransition("FINISHED", "Flower Box Up YN");
            flowerBoxYN.RemoveAction(1);
            flowerBoxYN.GetAction<SendEventByName>().sendEvent="BOX UP YN";
            flowerBoxYN.GetAction<Wait>().time = 0.25f;
            flowerBoxYN.Transitions=Array.Empty<FsmTransition>();
            flowerBoxYN.AddTransition("FINISHED", "Flower Text");
            sendFlowerText.Actions = new[]
            {
                new Vasi.InvokeMethod(() =>
                {
                    GameObject textYN=GameObject.Find("Text YN");
                    PlayMakerFSM control=textYN.LocateMyFSM("Dialogue Page Control");
                    control.FsmVariables.FindFsmInt("Toll Cost").Value=0;
                    control.FsmVariables.FindFsmGameObject("Requester").Value=qfsm.gameObject;
                    textYN.GetComponent<DialogueBox>().StartConversation(QuirrelLang.GiveKey,QuirrelLang.QuirrelSheet);
                })//Copy from https://github.com/flibber-hk/HollowKnight.MylaFlower
            };
            sendFlowerText.Transitions = Array.Empty<FsmTransition>();
            sendFlowerText.AddTransition("YES", "Accept");
            sendFlowerText.AddTransition("NO", "Decline");
            acceptBoxDown.Transitions = Array.Empty<FsmTransition>();
            acceptBoxDown.RemoveAction(1);
            acceptBoxDown.GetAction<SendEventByName>().sendEvent = "BOX DOWN YN";
            acceptBoxDown.AddTransition("FINISHED", "Change Tex");
            quirrelTexChange.AddAction(new Vasi.InvokeCoroutine(() => GiveFlower(qfsm.gameObject, giveclone), true));
            quirrelTexChange.Transitions= Array.Empty<FsmTransition>();
            quirrelTexChange.AddTransition("FINISHED", "Box Up 3");
            acceptBoxUp.Transitions = Array.Empty<FsmTransition>();
            acceptBoxUp.AddTransition("FINISHED", "Accept Flower");
            modifyconvostate(acceptConvo, QuirrelLang.AcceptKey);
            acceptConvo.Transitions= Array.Empty<FsmTransition>();
            acceptConvo.RemoveAction(3);
            acceptConvo.AddTransition("CONVO_FINISH", "Talk Finish");
            declineBoxDown.Transitions = Array.Empty<FsmTransition>();
            declineBoxDown.RemoveAction(1);
            declineBoxDown.GetAction<SendEventByName>().sendEvent = "BOX DOWN YN";
            declineBoxDown.AddTransition("FINISHED", "Box Up 2");
            declineBoxup.Transitions= Array.Empty<FsmTransition>();
            declineBoxup.AddTransition("FINISHED", "Decline Flower");
            modifyconvostate(declineConvo, QuirrelLang.DeclineKey);
            declineConvo.RemoveAction(3);
            declineConvo.Transitions= Array.Empty<FsmTransition>();
            declineConvo.AddTransition("CONVO_FINISH", "Talk Finish");

        }
        
        private IEnumerator GiveFlower(GameObject quirrel,GameObject effect)
        {
            HeroController.instance.gameObject.GetComponent<tk2dSpriteAnimator>().Play("Collect SD 1");
            yield return new WaitForSeconds(0.5f);
            LS.giveFlower = true;
            if(CKInstall())
            {
                CKReplace();
            }
            else
            {
                quirrel.GetComponent<tk2dSprite>().GetCurrentSpriteDef().material.mainTexture = QuirrelTex;
            }
            effect.SetActive(true);
            PlayerData.instance.SetBool(nameof(PlayerData.quirrelEpilogueCompleted), true);
            PlayerData.instance.SetBool(nameof(PlayerData.hasXunFlower), false);
        }
        private Texture2D LoadQuirrelTex()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach(var name in asm.GetManifestResourceNames())
            {
                if(name.EndsWith("Quirrel.png"))
                {
                    using Stream s= asm.GetManifestResourceStream(name);
                    byte[] buffer=new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                    Texture2D tex = new(2, 2);
                    tex.LoadImage(buffer);
                    s.Dispose();
                    return tex;
                }
            }
            return null;
        }
        private void modifyconvostate(FsmState state,string key)
        {
            CallMethodProper cmp = state.GetAction<CallMethodProper>();
            cmp.parameters[0].stringValue = key;
            cmp.parameters[1].stringValue = QuirrelLang.QuirrelSheet;
        }
        private void CKReplace()
        {
            var skin = SkinManager.GetCurrentSkin();
            Texture2D quirrelflowertex = skin.Exists("Quirrel_Flower.png") ? skin.GetTexture("quirrel_flower.png") : QuirrelTex;
            SkinManager.Skinables["Quirrel"].ApplyTexture(quirrelflowertex);
        }
        private bool CKInstall()
        {
            return ModHooks.GetMod("CustomKnight") is Mod;
        }
    }
}
