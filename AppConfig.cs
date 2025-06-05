namespace AIAgentWeb
{
    public class AppConfig
    {
        private string _ProjectEndpoint { get; set; }
        private string _VectorStoreId { get; set; }
        private string _AgentId { get; set; }
        private IConfiguration _config { get; set; }

        public AppConfig(IConfiguration config)
        {
            _ProjectEndpoint = config.GetValue<string>("ProjectEndpoint") ?? "";
            _VectorStoreId = config.GetValue<string>("VectorStoreId") ?? "vs_xxxx";
            _AgentId = config.GetValue<string>("AgentId") ?? "asst_xxxx";
            _config = config;
        }

        public string ProjectEndpoint
        {
            get => this._ProjectEndpoint;
            set => this._ProjectEndpoint = value;
        }


        public string VectorStoreId
        {
            get => this._VectorStoreId;
            set => this._VectorStoreId = value;
        }
        public string AgentId
        {
            get => this._AgentId;
            set => this._AgentId = value;
        }


        public IConfiguration config
        {
            get => this._config;
            set => this._config = value;
        }

    }
}   
