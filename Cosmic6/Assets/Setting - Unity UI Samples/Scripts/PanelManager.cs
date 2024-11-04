using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PanelManager : MonoBehaviour
{

	public Animator initiallyOpen;

	private int m_OpenParameterId;
	private Animator m_Open;
	private GameObject m_PreviouslySelected;

	const string k_OpenTransitionName = "Open";
	const string k_ClosedStateName = "Closed";

	public void OnEnable()
	{
		m_OpenParameterId = Animator.StringToHash(k_OpenTransitionName);

		if (initiallyOpen == null)
			return;

		OpenPanel(initiallyOpen);
	}

	public void OpenPanelAndPrepareLoad(Animator anim)
	{
		OpenPanel(anim);
		PrepareLoad();
	}

	public void OpenPanel(Animator anim)
	{
		if (m_Open == anim)
			return;

		anim.gameObject.SetActive(true);
		var newPreviouslySelected = EventSystem.current.currentSelectedGameObject;

		anim.transform.SetAsLastSibling();

		CloseCurrent();

		m_PreviouslySelected = newPreviouslySelected;

		m_Open = anim;
		m_Open.SetBool(m_OpenParameterId, true);

		GameObject go = FindFirstEnabledSelectable(anim.gameObject);

		SetSelected(go);
	}

	static GameObject FindFirstEnabledSelectable(GameObject gameObject)
	{
		GameObject go = null;
		var selectables = gameObject.GetComponentsInChildren<Selectable>(true);
		foreach (var selectable in selectables)
		{
			if (selectable.IsActive() && selectable.IsInteractable())
			{
				go = selectable.gameObject;
				break;
			}
		}
		return go;
	}

	public void CloseCurrent()
	{
		if (m_Open == null)
			return;

		m_Open.SetBool(m_OpenParameterId, false);
		SetSelected(m_PreviouslySelected);
		StartCoroutine(DisablePanelDeleyed(m_Open));
		m_Open = null;
	}

	IEnumerator DisablePanelDeleyed(Animator anim)
	{
		bool closedStateReached = false;
		bool wantToClose = true;
		while (!closedStateReached && wantToClose)
		{
			if (!anim.IsInTransition(0))
				closedStateReached = anim.GetCurrentAnimatorStateInfo(0).IsName(k_ClosedStateName);

			wantToClose = !anim.GetBool(m_OpenParameterId);

			yield return new WaitForEndOfFrame();
		}

		if (wantToClose)
			anim.gameObject.SetActive(false);
	}

	private void SetSelected(GameObject go)
	{
		EventSystem.current.SetSelectedGameObject(go);
	}

	public void PrepareLoad()
	{
		Debug.Log("(PanelManager) PrepareLoad");
		// Load 버튼 객체 찾기
		GameObject loadButton = GameObject.Find("Load");

		// 자식 중 "Dropdown" 이름을 가진 객체에서 Dropdown 컴포넌트 가져오기
		Dropdown dropdown = loadButton.transform.Find("Dropdown").GetComponent<Dropdown>();

		// JSON 파일을 저장하는 폴더 경로 설정
		string folderPath = Application.persistentDataPath;

		// 폴더에 있는 모든 JSON 파일 가져오기
		string[] files = Directory.GetFiles(folderPath, "*.json");

		// Dropdown의 옵션 초기화
		dropdown.ClearOptions();

		// 파일 이름 목록을 Dropdown 옵션 리스트로 변환
		List<string> options = new List<string>();
		foreach (string file in files)
		{
			string fileName = Path.GetFileNameWithoutExtension(file);
			options.Add(fileName);
		}

		// Dropdown에 옵션 추가
		dropdown.AddOptions(options);

		Debug.Log("JSON 파일 목록을 Dropdown에 로드했습니다.");
	}

	public void SaveGame()
	{
		// Save 버튼 객체 찾기
		GameObject saveButton = GameObject.Find("Save");

		// InputField 또는 TMP_InputField 중 실제 존재하는 것을 가져오기
		string fileName = null;

		// UnityEngine.UI.InputField 타입일 경우
		InputField inputField = saveButton.GetComponentInChildren<InputField>(true);
		if (inputField != null)
		{
			fileName = inputField.text;
			inputField.text = "";
		}
		else
		{
			Debug.Log("(PanelManager) InputField == null");
		}

		if (string.IsNullOrEmpty(fileName))
		{
			Debug.LogWarning("파일명을 입력하세요.");
			return;
		}

		// 저장할 데이터
		GameData gameData = new GameData
		{
			playerScore = 100, // 예시 데이터 TODO: 실제 데이터로 대체
			playerHealth = 75
		};

		string json = JsonUtility.ToJson(gameData);

		// JSON 파일 저장
		string filePath = Path.Combine(Application.persistentDataPath, fileName + ".json");
		File.WriteAllText(filePath, json);
		Debug.Log("게임 데이터가 저장되었습니다: " + filePath);

		// 패널 닫기
		CloseCurrent();
	}

	public void LoadGame()
	{
		// Load 버튼 객체 찾기
		GameObject loadButton = GameObject.Find("Load");

		// 자식 중 "Dropdown" 이름을 가진 객체에서 Dropdown 컴포넌트 가져오기
		Dropdown dropdown = loadButton.transform.Find("Dropdown").GetComponent<Dropdown>();

		// 선택된 파일 이름 가져오기
		if (dropdown.options.Count == 0)
		{
			Debug.LogWarning("로드할 파일이 없습니다.");
			return;
		}

		string selectedFileName = dropdown.options[dropdown.value].text;
		if (string.IsNullOrEmpty(selectedFileName))
		{
			Debug.LogWarning("선택된 파일명이 없습니다.");
			return;
		}

		// JSON 파일 경로 설정
		string filePath = Path.Combine(Application.persistentDataPath, selectedFileName + ".json");

		if (!File.Exists(filePath))
		{
			Debug.LogWarning("선택된 파일이 존재하지 않습니다: " + filePath);
			return;
		}

		// JSON 파일에서 데이터 읽기
		string json = File.ReadAllText(filePath);
		Debug.Log("게임 데이터가 로드되었습니다: " + json);

		// JSON 데이터를 변수에 할당
		GameData gameData = JsonUtility.FromJson<GameData>(json);
		int playerScore = gameData.playerScore;
		int playerHealth = gameData.playerHealth;

		// 불러온 데이터를 실제 게임 변수에 할당하는 부분 (TODO: 실제 게임 변수에 할당)
		Debug.Log("Player Score: " + playerScore);
		Debug.Log("Player Health: " + playerHealth);
	}

	// JSON 데이터를 저장하고 로드하기 위한 클래스
	[System.Serializable]
	public class GameData
	{
		public int playerScore;
		public int playerHealth;
	}
}
