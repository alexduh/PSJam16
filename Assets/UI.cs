using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class UI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI gunText;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] TextMeshProUGUI duplicateText;

    [SerializeField] GameObject deathMenu;
    [SerializeField] GameObject victoryMenu;

    [SerializeField] float startingTime = 60 * 10;
    [SerializeField] float time;

    [SerializeField] ExitDoor exitDoor;

    Player player;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = Player.Instance;
        time = startingTime;
    }

    // Update is called once per frame
    void Update()
    {
        
        timeText.text = ConvertSecondsToStringTime(time);
        gunText.text = player.weaponList[player.weaponIndex].weaponName;
        ammoText.text = "Ammo: " + player.weaponList[player.weaponIndex].curr_ammo.ToString();
        duplicateText.text = "Duplicates: " + player.sameWeaponTypeList.Count.ToString();

        if (time <= 0)
        {
            player.Death();
        }

        if (player.isDead)
        {
            deathMenu.SetActive(true);
        }
        else
        {
            time -= Time.deltaTime;
        }

        if (exitDoor.playerEscaped)
        {
            victoryMenu.SetActive(true);
        }

    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private string ConvertSecondsToStringTime(float inputTime)
    {
        int minutes = Mathf.FloorToInt((inputTime / 60F));
        //float seconds = Mathf.Round(inputTime - (minutes * 60F));

        float multiplier = Mathf.Pow(10, 2);
        float seconds = Mathf.Round((inputTime - (minutes * 60F)) * multiplier) / multiplier;

        string stringTime = minutes.ToString("00") + ":" + seconds.ToString("00.00");
        return stringTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

    }
}
