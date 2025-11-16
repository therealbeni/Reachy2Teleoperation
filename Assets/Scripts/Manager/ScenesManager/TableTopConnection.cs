using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TableTopConnection : MonoBehaviour
{
    public void LoadTabletopTeleoperationScene()
    {
        SceneManager.LoadScene("TabletopTeleoperationScene");
    }
}
