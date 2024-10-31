# Cosmic6

## 전체 Sprint 동안의 공지사항

- (10/26 유진) 우리가 만든 것은 Assets>Cosmic6 폴더 안에 넣어둡시다! 유니티 프로젝트 내부에서의 정리도 열심히 하고 원활한 협업을 위해 한 일과 사용법과 TODO 등도 Readme 등에 정리해두면 좋을 것 같아요!
- (10/31 유진) 현재 폴더 구조

```
- Assets
  - Cosmic6
    - Scenes
      - MainScene : (10/31 유진) 인벤토리와의 상호작용이 가능하게 만들어둔 다섯 에셋 (도끼/산소통/곡괭이/삽/통신장비) 그리고 간단한 배경이 들어있는 scene입니다. game mode에서 i key를 눌러 인벤토리를 활성화/비활성화할 수 있으며 앞서 언급한 물체들에 대해 클릭 및 드래그를 통해 인벤토리와 상호작용할 수 있는 상태입니다.
      - SettingsMenu : CitrioN 에셋이 적용된 설정 메뉴 scene입니다. (10/31 유진) 아직 별다른 설정이 들어가지는 않았습니다. Assets>Cosmic6>SettingsCitrioN>Settings>SettingsCollection.asset과 Assets>Cosmic6>SettingsCitrioN>Settings>UGUI>Menu>StyleProfile_UGUI_Default.asset 등을 통해 세부설정이 가능합니다. 버튼을 통한 scene 간 이동도 구현돼야 할 것 같습니다.
    - Prefabs : 인벤토리와의 상호작용이 안 들어가 있는 상태의 prefab들
    - PrefabsForInventory : scene에 instantiate하면 인벤토리와의 상호작용이 가능한 아이템들. 인벤토리 데이터베이스에 등록된 아이템에 대응되는, setup을 통해 생성된 prefab들이므로 인벤토리 매니저를 통하지 않고 위치를 바꾸지는 맙시다.
  - FBXExportForSprites : 인벤토리와 상호작용가능한 아이템에게 sprites라는 이름의 아이콘을 할당해줘야 하는데, 그걸 렌더링하고자 만든 임시 폴더입니다. 신경쓰지 않으셔도 됩니다.
  - Inventory - Devion Games : 인벤토리 에셋입니다. 여기에 두는 게 좋을 것 같습니다.
  - Plants - Splash of Color : 식물 에셋입니다. 여기에 두는 게 좋을 것 같습니다.
  - Setting - CitrioN : 설정 에셋입니다. 여기에 두는 게 좋을 것 같습니다.
  - TextMesh Pro : TMP를 import했더니 생긴 폴더입니다. 여기에 두는 게 좋을 것 같습니다.

```

## Sprint 1

### UI

#### Inventory

- 링크
  - Unity Asset Store - https://assetstore.unity.com/packages/tools/gui/item-inventory-system-45568
  - 개발사의 유튜브 튜토리얼 - https://www.youtube.com/watch?v=bz1Gm-l1OXA&list=PLexJx2VysToY4jOF_KsS054dXRgfyw45X&pp=iAQB
- 10/26~31 유진
  - 한 일: Inventory Asset import 후 개발사의 유튜브 튜토리얼 재생목록을 찾았으며 그 중 첫 영상인 Quickstart 영상 https://www.youtube.com/watch?v=bz1Gm-l1OXA 을 참고해 필수품들인 농기구, 산소통, 통신장비를 등록 후 테스트 완료함
  - MainScene에서 game 버튼을 누른 후 i 버튼을 누르면 inventory가 열린다. asset을 클릭하면 inventory에 들어가고, inventory에 있는 icon을 클릭하면 asset을 다시 inventory 밖으로 뺄 수 있다!
  - 개발하다보면 database를 선택해야 되는 란이 있다. 한눈에 알아보기 쉽도록 우리 것은 Cosmic6Database라고 지정하였다
- 10/31 기준 TODO
  - 인벤토리와의 상호작용이 필요한 asset들을 카테고리별로 정리해서 inventory 구축하는 나머지 과정 진행. 우리의 게임기획과 inventory 구현이 충분히 align되어야 함!
  - icon 즉 sprite 만들어서 적절히 배치하기. 인벤토리 상호작용 애셋 개수가 늘어날 경우 어떻게 빠르게 자동화해서 진행할 수 있는지 방법 찾기.

##### Plant

- 링크
  - Unity Asset Store - https://assetstore.unity.com/packages/3d/vegetation/plants/lite-splash-of-color-unique-photogrammetry-plants-214635?locale=ko-KR
- 10/26 유진
  - Import 후 Inventory에 한개 넣어둘 수 있도록 구현했습니다 아이콘은 아직 이상하지만요! (Inventory Quickstart)

#### Settings

- 유용한 링크들
  - Unity Asset Store - https://assetstore.unity.com/packages/tools/gui/settings-menu-creator-268863
  - Documentation - https://drive.google.com/drive/folders/14OkjS6XxyINqEVRxr7kXIEGlcIQzIM6e
  - Website - https://sites.google.com/view/citrion/settings-menu-creator?authuser=0
- 10/31 유진
  - Assets>Cosmic6>Scenes>SettingsMenu 라는 이름의 scene에 초기 설정화면이 나오는것까지 해뒀습니다 기획 세부사항 확정하면 빠르게 구현 가능할 것 같아요
