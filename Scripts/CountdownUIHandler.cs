using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownUIHandler : MonoBehaviour
{
    public TextMeshProUGUI countDownText;

    private void Awake()
    {
        countDownText.text = "";
    }

    void Start()
    {
        StartCoroutine(CountDownCO());
    }

    IEnumerator CountDownCO()
    {
        yield return new WaitForSeconds(0.3f);

        int counter = 3;

        while (counter >= 0)
        {
            if (counter > 0)
            {
                countDownText.text = counter.ToString();
            }
            else
            {
                countDownText.text = "GO";
            }

            counter--;
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);
    }
}