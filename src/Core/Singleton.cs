
namespace ZargoEngine
{
    public class Singleton<T> where T : MonoBehaviour
    {
        private static T _instance;
        public static T instance
        {
            get
            {
                if (_instance == null){
                    _instance = SceneManager.FindObjectOfType<T>();
                }
                return _instance;   
            }
        }
    }
}
