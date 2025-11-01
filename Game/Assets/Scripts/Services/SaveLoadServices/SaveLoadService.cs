using UnityEngine;

namespace Services.SaveLoadServices
{
    public class SaveLoadService : ISaveLoadService
    {
        private GameProgress _gameProgress;
        private readonly PlayerPrefsStorage<GameProgress> _playerPrefsStorage;
        
        private readonly string _playerProgressKey = "GameProgress";

        public SaveLoadService() =>
            _playerPrefsStorage = new PlayerPrefsStorage<GameProgress>(_playerProgressKey);

        public void Save() => _playerPrefsStorage.Save(_gameProgress);

        public void Load() => _gameProgress = _playerPrefsStorage.Load() ?? new GameProgress();

        public GameProgress GetProgress() => _gameProgress;

        public void ClearSave() => PlayerPrefs.DeleteAll();
    }
}