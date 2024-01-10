using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;
using UnityEngine.UI;

namespace anypractice
{
    internal class Timer:MonoBehaviour
    {
        private bool start=false;
        private bool started=false;
        private bool win = false;
        private bool over=false;
        private bool overed = false;
        PlayMakerFSM _con;
        PlayMakerFSM bc;
        SetBoolValue Ove=new();
        HealthManager _hm;


        private void Awake()
        {
            _con = base.gameObject.LocateMyFSM("Control");
            bc = GameObject.Find("Boss Control").LocateMyFSM("Control");
            if (anypractice.Instance._set.timer == 1)
            {
                anypractice.Instance._th.time = 0f;
            }
           //Modding.Logger.Log("CREATE OK");
            On.HealthManager.TakeDamage += knighthit;
            _hm=base.GetComponent<HealthManager>();
        }
        private void Stop()
        {
            if (anypractice.Instance._th.start)
            {
                anypractice.Instance._th.over = true;
            }
            anypractice.Instance.count++;
        }
        private void Start()
        {
            anypractice.Instance._th.start = false;
            _con.InsertCustomAction("Final Impact", Stop, 0);
            bc.InsertCustomAction("Flash Down", () =>
            {
                StartCoroutine(starttimer());
            }, 4);
            anypractice.Instance._th.over = false;
        }

        private IEnumerator starttimer()
        {
            yield return new WaitForSeconds(0.5f);
            start=true;
        }
        private void knighthit(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (!start)
            {
                start = true;
            }
            orig(self, hitInstance);
        }

        private void Update()
        {
            if(start&!started)
            {
                started= true;
                anypractice.Instance._th.start=true;
            }


        }
        private void OnDestroy() 
        {
            On.HealthManager.TakeDamage -= knighthit;
            anypractice.Instance._th.start=false;
            anypractice.Instance._th.over=false;
            if (!win) anypractice.Instance.count = 0;
        }
    }
    public class timerhelp:MonoBehaviour
    {
        TimeSpan timeSpan { get; set; }
        //Time time = new Time();
        public float time= new float();
        public bool start = false;
        public bool over = false;
        public GameObject timeCanvas;
        public Text text;
        private Vector2 timeposition=new Vector2(1880,1020);
        private void Awake()
        {
            GameObject t = GameObject.Find("timeCanvas");
            if(t != null) { Destroy(t); }
            if (timeCanvas != null)
            {
                DestroyImmediate(timeCanvas);
            }
            timeCanvas = CanvasUtil.CreateCanvas(UnityEngine.RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            timeCanvas.SetActive(true);
            time = 0;
            text=CanvasUtil.CreateTextPanel(
                timeCanvas,
                TimerText(),
                40,
                TextAnchor.MiddleLeft,
                new CanvasUtil.RectData(new Vector2(700f, 100f), new Vector2(-560f, 805f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f)),
                //CreateTimeRecvData(new Vector2(240,40),new Vector2()),
                true
                ).GetComponent<Text>();
               UnityEngine.Object.DontDestroyOnLoad(timeCanvas);
            text.color = Color.white;
            Modding.Logger.Log("CANVAS   OK");
        }

        private CanvasUtil.RectData CreateTimeRecvData(Vector2 size,Vector2 selPosition)
        {
            return new CanvasUtil.RectData(
                size,
                timeposition + selPosition,
                -new Vector2(),
                new Vector2(1, 0.5f)
                );
        }

        private string TimerText()
        {
            //Modding.Logger.Log("now");
            return string.Format(
                "{0}:{1:D2}:{2:D3}\n{4}",
                timeSpan.Minutes,
                timeSpan.Seconds,
                timeSpan.Milliseconds,
                anypractice.Instance.count
                );
        }

        private void Update()
        {
            if (start&!over)
            {
                if (!GameManager.instance.isPaused)
                    time += Time.unscaledDeltaTime;
            }
            timeSpan = TimeSpan.FromSeconds(time);
            text.text= TimerText();
            //Modding.Logger.Log("text"+text.text);
        }

        private void OnDestroy()
        {
            Destroy(timeCanvas);
        }
    }
}