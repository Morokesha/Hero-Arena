namespace Services.SaveLoadServices
{
    public interface ISaveLoadService
    {
        public void Save();
        public void Load();
        public void ClearSave();
        public GameProgress GetProgress();
    }
}