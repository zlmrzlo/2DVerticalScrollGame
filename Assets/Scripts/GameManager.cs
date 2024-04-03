using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.TextCore.Text;


public class GameManager : MonoBehaviour
{
    public int stage;
    public Animator stageAnim;
    public Animator clearAnim;
    public Animator fadeAnim;
    public Transform playerPos;

    public string[] enemyObjs;
    public Transform[] spawnPoints;

    public float nextSpawnDelay;
    public float curSpawnDelay;

    public GameObject player;
    public Text scoreText;
    public Image[] lifeImage;
    public Image[] boomImage;
    public GameObject gameOverSet;
    public ObjectManager objectManager;

    public List<Spawn> spawnList;
    public int spawnIndex;
    public bool spawnEnd;

    void Awake()
    {
        spawnList = new List<Spawn>();
        enemyObjs = new string[] { "EnemyS", "EnemyM", "EnemyL", "EnemyB" };
        StageStart();
    }

    public void StageStart()
    {
        //#. Stage UI Load
        stageAnim.SetTrigger("On");
        stageAnim.GetComponent<Text>().text = "Stage " + stage + "\nStart";
        clearAnim.GetComponent<Text>().text = "Stage " + stage + "\nClear!!";
        //#.Enemy Spawn File Read
        ReadSpawnFile();

        //#.Fade in
        fadeAnim.SetTrigger("In");
    }

    public void StageEnd()
    {
        //#.Clear UI Load
        clearAnim.SetTrigger("On");

        //#.Fade Out
        fadeAnim.SetTrigger("Out");
        //#.Player Repos
        player.transform.position = playerPos.position;

        //#.Stage Increament
        stage++;
        if (stage > 2)
            Invoke("GameOver",6);
        else
            Invoke("StageStart", 5);
    }

    void ReadSpawnFile()
    {
        //#1. 변수 초기화
        spawnList.Clear();
        spawnIndex = 0;
        spawnEnd = false;

        //#2. 리스폰 파일 열기
        UnityEngine.TextAsset textFile = Resources.Load("Stage " + stage.ToString()) as UnityEngine.TextAsset;
        StringReader stringReader = new StringReader(textFile.text); ;

        while(stringReader != null)
        {
            string line = stringReader.ReadLine();
            Debug.Log(line);

            if (line == null)
                break;

            //#.리스폰 데이터 생성
            Spawn spawnDate = new Spawn();
            spawnDate.delay = float.Parse(line.Split(',')[0]);
            spawnDate.type = line.Split(',')[1];
            spawnDate.point = int.Parse(line.Split(',')[2]);
            spawnList.Add(spawnDate);
        }

        //#.텍스트 파일 닫기
        stringReader.Close();

        //#첫번째 스폰 딜레이 적용
        nextSpawnDelay = spawnList[0].delay;

        


    }

    void Update()
    {
        curSpawnDelay += Time.deltaTime;

        if(curSpawnDelay > nextSpawnDelay  && !spawnEnd)
        {
            SpawnEnemy();
            curSpawnDelay = 0;      // 적 생성 후엔 꼭 딜레이 변수 0으로 초기화
        }

        // #. UI Score Update
        Player playerLogic = player.GetComponent<Player>();
        scoreText.text = string.Format("{0:n0}", playerLogic.score);
    }

    // 랜덤으로 정해진 적 프리펩, 생성 위치로 적 기체 생성.
    void SpawnEnemy()
    {
        int enemyIndex = 0;
        switch (spawnList[spawnIndex].type)
        {
            case "S":
                enemyIndex = 0;
                break;
            case "M":
                enemyIndex = 1;
                break;
            case "L":
                enemyIndex = 2;
                break;
            case "B":
                enemyIndex = 3;
                break;

        }
        

        int enemyPoint = spawnList[spawnIndex].point;

        GameObject enemy = objectManager.MakeObj(enemyObjs[enemyIndex]);
        enemy.transform.position = spawnPoints[enemyPoint].position;

        Rigidbody2D rigid = enemy.GetComponent<Rigidbody2D>();
        Enemy enemyLogic = enemy.GetComponent<Enemy>();
        enemyLogic.player = player;     // 프리펩은 이미 Scene에 올라온 오브젝트에 접근 불가능!! 그래서 Enemy에서 바로 player를 못받는다  
                                        // ==> 적 생성 직후 플레이어 변수를 넘겨주는 것으로 해결가능하다!!
        enemyLogic.gameManager = this;  
        enemyLogic.objectManager = objectManager;
        if (enemyPoint == 5 || enemyPoint == 6) // #.Right Spawn
        {
            enemy.transform.Rotate(Vector3.back * 45);
            rigid.velocity = new Vector2(enemyLogic.speed * (-1), -1);
        }
        else if (enemyPoint == 7 || enemyPoint == 8)    // #.Left Spawn
        {
            enemy.transform.Rotate(Vector3.forward * 45);
            rigid.velocity = new Vector2(enemyLogic.speed, -1);
        }
        else    //#. Frong Spawn
        {
            rigid.velocity = new Vector2(0, enemyLogic.speed * (-1));
        }

        //#.리스폰 인덱스 증가
        spawnIndex++;
        if(spawnIndex == spawnList.Count)
        {
            spawnEnd = true;
            return;
        }
        //#.다음 리스폰 딜레이 갱신
        nextSpawnDelay = spawnList[spawnIndex].delay;
    }
    public void UpdateLifeIcon(int life)
    {
        // Image를 일단 모두 투명 상태로 두고, 목숨대로 반투명 설정.

        // #.UI Life Init Disable
        for (int i=0; i<3; i++)
        {
            lifeImage[i].color = new Color(1, 1, 1, 0);
        }

        // #.UI Life Active
        for(int i=0; i<life; i++)
        {
            lifeImage[i].color = new Color(1, 1, 1, 1);
        }
    }

    public void UpdateBoomIcon(int boom)
    {
        // Image를 일단 모두 투명 상태로 두고, 목숨대로 반투명 설정.

        // #.UI Boom Init Disable
        for (int i = 0; i < 3; i++)
        {
            boomImage[i].color = new Color(1, 1, 1, 0);
        }

        // #.UI Boom Active
        for (int i = 0; i < boom; i++)
        {
            boomImage[i].color = new Color(1, 1, 1, 1);
        }
    }
    public void RespawnPlayer()
    {
        Invoke("RespawnPlayerExe", 2f);    // 플레이어 복귀는 시간 차를 두기 위해 Invoke() 사용
    }

    void RespawnPlayerExe()
    {
        player.transform.position = Vector3.down * 3.5f;
        player.SetActive(true);

        Player playerLogic = player.GetComponent<Player>();
        playerLogic.isHit = false;
    }

    public void CallExplosion(Vector3 pos, string type)
    {
        GameObject explosion = objectManager.MakeObj("Explosion");
        Explosion explosionLogic = explosion.GetComponent<Explosion>();

        explosion.transform.position = pos;
        explosionLogic.StartExplosion(type);
    }

    public void GameOver()
    {
        gameOverSet.SetActive(true);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }

}
