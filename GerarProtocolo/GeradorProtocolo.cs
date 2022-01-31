using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerarProtocolo
{
    public class GeradorProtocolo : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)); // Estamos gerando o Context (linha Padrão)

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity ocorrencia = (Entity)context.InputParameters["Target"];
                if(ocorrencia.LogicalName == "incident")
                {
                    string clienteNome = null;
                    try
                    {
                        // clienteNome = ocorrencia.GetAttributeValue<EntityReference>("customerid").Id.ToString();
                        Entity opportunity = service.Retrieve("contact", ocorrencia.GetAttributeValue<EntityReference>("customerid").Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("firstname"));
                        clienteNome = opportunity.Attributes["firstname"].ToString();
                    }
                    catch(Exception ex)
                    {
                        throw new InvalidPluginExecutionException($"Não foi possível localizar o cliente. Erro: {ex.Message}");
                    }

                    clienteNome = clienteNome.Substring(0, 3);
                    DateTime dataAtual = DateTime.Now;
                    string numeroDoProtocolo = ($"{dataAtual.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss")}{clienteNome.ToUpper()}");
                    ocorrencia["gp_numeroprotocolo"] = numeroDoProtocolo;
                }
            }
        }
    }
}
