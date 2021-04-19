using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
public class MazeController : MonoBehaviour
{
    // Start is called before the first frame update
    //static reference
    public static MazeController script;
    public Text seedTxt;
    public Text sizeTxt;
    public Text layerTxt;
    public MazeGenerator mg;
    public Animator anim;
    public Button btn1;
    public Button btn2;
    public GameObject player;
    // --- banner settings ---
    public string[] quips;
    public Text quipText;
    public bool bannerVisible = false;
    public Color[] bannerColor;
    public int bannerState = 0;
    public Text bannerText;
    public Image bannerBkgrnd;
    public GameObject mazeGenerator_prefab;
    public GameObject mazeSolver_prefab;
    public GameObject mazeGenerator;
    public GameObject mazeSolver;
    public int numberOfRuns = 10;
    public int currentRun = 0;
    public int[] sizes;
    public int layers;
    public int currentSize = 0;
    
    public GameObject menuPanel;
    public long[] frameJumps;
    // --- control panel ---
    public Text tts;
    public GameObject FPV_Camera;
    public GameObject menuCamera;
    public Canvas UI;
    public Slider zoomSlider;
    public Slider speedSlider;
    public Slider menuSpeedSlider;
    public float initSolverSpeed = 15f;
    public GameObject endMenu;
    void Start()
    {
        endMenu.SetActive(false);
        script = this;
        Time.timeScale = 1;
        mg = FindObjectOfType<MazeGenerator>();
        setBannerState(0);
        //StartCoroutine(startRun());
        int randQ = Random.Range(0, quips.Length);
        quipText.text = quips[randQ];
        int s = Random.Range(0, frameJumps.Length);
        player.GetComponent<VideoPlayer>().frame = frameJumps[s];
    }

    public void gen() {
        StartCoroutine(genMaze());
    }
    //accessable via ui
    public IEnumerator genMaze() {
        if (mazeGenerator != null) {
            Destroy(mazeGenerator);
        }
        mazeGenerator = Instantiate(mazeGenerator_prefab, Vector3.zero, mazeGenerator_prefab.transform.rotation);
        //mazeSolver = Instantiate(mazeSolver_prefab, Vector3.zero, mazeSolver_prefab.transform.rotation);
        mazeGenerator.GetComponent<MazeGenerator>().setSize(sizeTxt.text);
        mazeGenerator.GetComponent<MazeGenerator>().setLayers(layerTxt.text);
        //float timeToFinish = (((Mathf.Pow(mData.size, 2) * 19) * mData.numLayers) / 60) / movementSpeed;
        float halfSize = (mazeGenerator.GetComponent<MazeGenerator>().data.size / 2) * 10;
        menuCamera.GetComponent<cameraScript>().rotateTarget = new Vector3(mazeGenerator.transform.position.x + halfSize, mazeGenerator.transform.position.y, mazeGenerator.transform.position.z + halfSize);
        menuCamera.GetComponent<cameraScript>().rotateAroundObject = true;
        menuCamera.transform.position = new Vector3(0, mazeGenerator.transform.position.y + (halfSize), mazeGenerator.transform.position.z + (halfSize));
        yield return StartCoroutine(generateMaze());

    }

    //this is used for automated serries of mazes, not sure if this will be exposed to end users via UI or just for development purposes
    public IEnumerator startRun() {
        if (menuPanel.activeSelf) {
            menuPanel.SetActive(false);
        }
        mazeGenerator = Instantiate(mazeGenerator_prefab, Vector3.zero, mazeGenerator_prefab.transform.rotation);
        mazeSolver = Instantiate(mazeSolver_prefab, Vector3.zero, mazeSolver_prefab.transform.rotation);
        mazeGenerator.GetComponent<MazeGenerator>().setSize(sizes[currentSize].ToString());
        mazeGenerator.GetComponent<MazeGenerator>().setLayers(layers.ToString());
        yield return StartCoroutine(generateMaze());
        mazeSolver.SetActive(true);
        mazeSolver.GetComponent<SolverAI>().MoveToStartPosition();
        mazeSolver.GetComponent<SolverAI>().serriesSolver = true;
        setBannerState(1);
        tts.text = tts.text + FindObjectOfType<SolverAI>().GetTimeToSolve() + " minutes.";
    }

    public void endRun() {
        
        Destroy(mazeGenerator);
        Destroy(mazeSolver);
        if (currentRun < numberOfRuns)
        {
            StartCoroutine(startRun());
            currentRun++;
        }
        else if (currentSize < sizes.Length-1) {
            currentSize++;
            currentRun = 0;
            StartCoroutine(startRun());
        }
    }
    public void setBannerState(int newState) {
        if (newState > 0 && newState < bannerColor.Length) {
            bannerState = newState;
        }
        switch (bannerState) {
            case 0:
                //init
                bannerText.text = "";
                bannerBkgrnd.color = bannerColor[bannerState];
                break;
            case 1:
                //solving maze
                bannerText.text = "SOLVING IN PROGRESS";
                bannerBkgrnd.color = bannerColor[bannerState];
                break;
            case 2:
                //solution replay
                bannerText.text = "SOLUTION DISCOVERED";
                bannerBkgrnd.color = bannerColor[bannerState];
                endMenu.SetActive(true);

                break;
        }
    }

    public IEnumerator generateMaze() {
        yield return StartCoroutine(mazeGenerator.GetComponent<MazeGenerator>().CreateMaze());
        yield return null;
    }
    public void SetSeed() {
        mg.setSeed(seedTxt.text);
        mg.setSize(sizeTxt.text);
        mg.setLayers(layerTxt.text);
    }

    public void StartSolver() {
        mazeSolver = Instantiate(mazeSolver_prefab, Vector3.zero, mazeSolver_prefab.transform.rotation);
        mazeSolver.SetActive(true);
        mazeSolver.GetComponent<SolverAI>().movementSpeed = initSolverSpeed;
        speedSlider.value = initSolverSpeed;
        mazeSolver.GetComponent<SolverAI>().MoveToStartPosition();
        menuCamera.GetComponent<cameraScript>().rotateAroundObject = false;
        FPV_Camera = mazeSolver.transform.GetChild(0).gameObject;
        menuCamera.GetComponent<cameraScript>().target = mazeSolver;
        player.SetActive(false);
        anim.SetBool("Solver_Started", true);
        setBannerState(1);
        tts.text = tts.text + FindObjectOfType<SolverAI>().GetTimeToSolve() + " minutes.";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setFPVCamera() {
        menuCamera.GetComponent<Camera>().enabled = false;
        FPV_Camera.GetComponent<Camera>().enabled = true;
        //UI.worldCamera = FPV_Camera.GetComponent<Camera>();
    }

    public void setStaticWoldCamera() {
        menuCamera.GetComponent<Camera>().enabled = true;
        FPV_Camera.GetComponent<Camera>().enabled = false;
        //UI.worldCamera = menuCamera.GetComponent<Camera>();
    }
    public void setFollowCamera() {
        menuCamera.GetComponent<Camera>().enabled = true;
        FPV_Camera.GetComponent<Camera>().enabled = false;
        //UI.worldCamera = menuCamera.GetComponent<Camera>();
    }

    public void UpdateCameraZoom() {
        menuCamera.GetComponent<cameraScript>().floatDistance = zoomSlider.value;
    }

    public void UpdateSolverSpeed() {
        mazeSolver.GetComponent<SolverAI>().movementSpeed = speedSlider.value;
    }

    public void UpdateSolverSpeedMenu()
    {
        initSolverSpeed = menuSpeedSlider.value;
    }

    public void reload() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

}
