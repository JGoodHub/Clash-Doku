using System.Collections;
using Async.Connector;
using GoodHub.Core.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameController : GlobalSingleton<GameController>
{

    [SerializeField] private MatchConfig _standardMatchConfig;

    private MatchReport _activeMatchReport;
    private MatchConfig _activeMatchConfig;

    public MatchReport ActiveMatchReport => _activeMatchReport;

    public MatchConfig ActiveMatchConfig => _activeMatchConfig;

    public MatchConfig StandardMatchConfig => _standardMatchConfig;

    protected override void Awake()
    {
        base.Awake();
        AsyncConnector.InitialiseAndLogin(true);
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);

        yield return new WaitUntil(() => AsyncConnector.LoggedIn);

        // Load the menu scene

        SceneManager.LoadScene(1);
    }

    public void LoadIntoMatchScene(MatchReport matchReport, MatchConfig matchConfig)
    {
        _activeMatchReport = matchReport;
        _activeMatchConfig = matchConfig;

        SceneManager.LoadScene(2);
    }

    public void LoadHomeScene()
    {
        _activeMatchReport = null;
        SceneManager.LoadScene(1);
    }

}
