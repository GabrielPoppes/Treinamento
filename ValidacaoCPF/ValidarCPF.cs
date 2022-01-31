using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ValidacaoCPF
{
    public class ValidarCPF
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)); // Estamos gerando o Context (linha Padrão)

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "gbrmad_cliente") // Contact lógico da tabela Cliente
            {
                Entity cliente = (Entity)context.InputParameters["Target"];
                string cpfCliente = cliente["gbrmad_strcpf"].ToString(); // Pegou o valor digitado na coluna CPF
                if (cpfCliente != null)
                {
                    Regex regex = new Regex(@"([0-9]{2}[\.]?[0-9]{3}[\.]?[0-9]{3}[\/]?[0-9]{4}[-]?[0-9]{2})|([0-9]{3}[\.]?[0-9]{3}[\.]?[0-9]{3}[-]?[0-9]{2})");
                    Match match = regex.Match(cpfCliente);
                    if (match.Success)
                    {

                    }

                    else
                    {
                        throw new InvalidPluginExecutionException("Erro! Este CPF é inválido!");
                    }
                }

                else
                {
                    throw new InvalidPluginExecutionException("Erro! Este CNPJ é inválido!");
                }
            }
        }
    }
}
