using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class HospitalUIController : MonoBehaviour
{
    [Header("===== 挂号端UI =====")]
    public TMP_InputField inputPatientName;
    public TMP_Dropdown dropdownPriority;
    public Button btnRegister;

    [Header("===== 排队显示端UI =====")]
    public TextMeshProUGUI textWaitingList;

    [Header("===== 医生就诊端UI =====")]
    public TextMeshProUGUI textDoctorStatus;
    public Button btnStartTreat;
    public Button btnFinishTreat;

    [Header("===== 统计/时间控制UI =====")]
    public TextMeshProUGUI textSimTime;
    public TextMeshProUGUI textStatInfo;
    public Button btnTimeAddOne;
    public Toggle toggleAutoRun;
    public Button btnReset; // 新增：重置按钮

    // 自动运行固定速度：每1秒仿真时间前进1
    private readonly float autoStepInterval = 1f;
    private float autoTimer;

    // 核心对象
    private Priority_queue waitHeap;
    private SlidingQueue slidingQueue;

    // 全局唯一虚拟仿真时间
    private int simNow;
    private const int WindowSize = 30;
    private int patientIdCounter = 1;

    // 医生状态
    private bool doctorBusy;
    private Patient currentTreatPatient;
    private int treatStartTime;

    void Start()
    {
        // 绑定按钮事件
        btnRegister.onClick.AddListener(OnRegisterClick);
        btnStartTreat.onClick.AddListener(OnStartTreatClick);
        btnFinishTreat.onClick.AddListener(OnFinishTreatClick);
        btnTimeAddOne.onClick.AddListener(OnTimeAddOneClick);
        toggleAutoRun.onValueChanged.AddListener(OnAutoRunToggleChanged);
        btnReset.onClick.AddListener(OnResetClick); // 绑定重置按钮

        InitSimulation();
    }

    // 初始化/重置仿真状态
    void InitSimulation()
    {
        waitHeap = new Priority_queue();
        slidingQueue = new SlidingQueue(WindowSize);
        simNow = 0;
        doctorBusy = false;
        currentTreatPatient = null;
        treatStartTime = 0;
        patientIdCounter = 1;
        autoTimer = 0;
        toggleAutoRun.isOn = false; // 重置时关掉自动运行
        inputPatientName.text = "";
        RefreshAllUI();
    }

    // 重置按钮点击事件
    void OnResetClick()
    {
        InitSimulation();
        Debug.Log("仿真已重置");
    }

    void Update()
    {
        if (toggleAutoRun.isOn)
        {
            autoTimer += Time.deltaTime;
            if (autoTimer >= autoStepInterval)
            {
                AdvanceSimTime();
                autoTimer = 0;
            }
        }
    }

    void OnAutoRunToggleChanged(bool isOn)
    {
        autoTimer = 0;
    }

    // ============ 挂号端 ============
    void OnRegisterClick()
    {
        string name = inputPatientName.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("姓名不能为空");
            return;
        }
        int prio = int.Parse(dropdownPriority.options[dropdownPriority.value].text);

        Patient p = new Patient()
        {
            id = patientIdCounter++,
            name = name,
            priority = prio,
            arrivetime = simNow
        };
        waitHeap.Push(p);
        inputPatientName.text = "";
        RefreshAllUI();
    }

    // ============ 医生就诊端 ============
    void OnStartTreatClick()
    {
        if (doctorBusy) return;
        if (waitHeap.Empty())
        {
            Debug.Log("当前没有排队病人");
            return;
        }
        currentTreatPatient = waitHeap.Pop();
        treatStartTime = simNow;
        doctorBusy = true;
        RefreshAllUI();
    }

    void OnFinishTreatClick()
    {
        if (!doctorBusy || currentTreatPatient == null) return;

        Record rec = new Record()
        {
            finishtime = simNow,
            waittime = treatStartTime - currentTreatPatient.arrivetime,
            treattime = simNow - treatStartTime,
            priority = currentTreatPatient.priority
        };
        slidingQueue.Push(rec);

        doctorBusy = false;
        currentTreatPatient = null;
        RefreshAllUI();
    }

    // ============ 虚拟时间控制 ============
    void OnTimeAddOneClick()
    {
        AdvanceSimTime();
    }

    void AdvanceSimTime()
    {
        simNow++;
        slidingQueue.Update(simNow);
        RefreshAllUI();
    }

    // ============ UI刷新 ============
    void RefreshAllUI()
    {
        RefreshWaitingList();
        RefreshDoctorUI();
        RefreshStatUI();
    }

    void RefreshWaitingList()
    {
        var all = waitHeap.GetAllPatients();
        var sorted = all
            .OrderByDescending(x => x.priority)
            .ThenBy(x => x.arrivetime)
            .Take(5)
            .ToList();

        string str = "===== 排队病人=====\n";
        foreach (var p in sorted)
        {
            str += $"ID:{p.id} | {p.name} | 优先级:{p.priority} | 到达时间:{p.arrivetime}\n";
        }
        textWaitingList.text = str;
    }

    void RefreshDoctorUI()
    {
        if (doctorBusy && currentTreatPatient != null)
        {
            textDoctorStatus.text =
                $"<color=red>【就诊中】\n</color>病人：{currentTreatPatient.name}\n开始就诊时间：{treatStartTime}";
            btnStartTreat.interactable = false;
            btnFinishTreat.interactable = true;
        }
        else
        {
            textDoctorStatus.text = "<color=green>【医生空闲】\n</color>等待接诊病人";
            btnStartTreat.interactable = !waitHeap.Empty();
            btnFinishTreat.interactable = false;
        }
    }

    void RefreshStatUI()
    {
        textSimTime.text = $"当前时间：{simNow}";
        var list = slidingQueue.Getlist();
        int count = list.Count;

        if (count == 0)
        {
            textStatInfo.text =
                $"滑动窗口大小：{WindowSize}\n窗口内完成就诊人数：0\n暂无统计数据";
            return;
        }

        int sumWait = 0;
        int sumTreat = 0;
        foreach (var r in list)
        {
            sumWait += r.waittime;
            sumTreat += r.treattime;
        }
        double avgWait = (double)sumWait / count;
        double avgTreat = (double)sumTreat / count;

        textStatInfo.text =
            $"滑动窗口大小：{WindowSize}\n" +
            $"窗口内完成就诊人数：{count}\n" +
            $"平均等待时间：{avgWait:F2}\n" +
            $"平均就诊时长：{avgTreat:F2}";
    }
}
