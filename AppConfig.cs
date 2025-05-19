namespace AIAgentWeb
{
    public class AppConfig
    {
        private string _ProjectCS { get; set; }
        private string _VectorStoreId { get; set; }

        private string _AgentId { get; set; }
        private IConfiguration _config { get; set; }


        public AppConfig(IConfiguration config)
        {
            _ProjectCS = config.GetValue<string>("ProjectConnectionString") ?? "";
            _VectorStoreId = config.GetValue<string>("VectorStoreId") ?? "vs_xxxx";
            _AgentId = config.GetValue<string>("AgentId") ?? "asst_xxxx";
            _config = config;
        }

        public string ProjectCS
        {
            get => this._ProjectCS;
            set => this._ProjectCS = value;
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
