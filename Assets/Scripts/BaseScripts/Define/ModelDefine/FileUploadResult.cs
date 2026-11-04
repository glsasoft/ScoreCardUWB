namespace LetsPiu.Base.BaseScripts.Define.ModelDefine
{
    public class FileUploadResult
    {
        public string url;
        public FileUploadError error;

        public FileUploadResult(string url, FileUploadError error)
        {
            this.url = url;
            this.error = error;
        }
    }

    public enum FileUploadError
    {
        NotLogin,
        TypeNotSupport,
        CosAuthError,
        UploadFail,
        Success
    }
    
    public enum ImageUploadType
    {
        UserHead,
        UserImage,
    }
}