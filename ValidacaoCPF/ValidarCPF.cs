using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    QueryExpression queryContact = new QueryExpression(cliente.LogicalName);
                    queryContact.ColumnSet.AddColumn("gp_cnpj");
                    queryContact.Criteria.AddCondition("gp_cnpj", ConditionOperator.Equal, cpfCliente);
                    EntityCollection collectioncontact = service.RetrieveMultiple(queryContact);

                    if (collectioncontact.Entities.Count() > 0)
                    {
                        throw new InvalidPluginExecutionException("Este CNPJ já está cadastrado!");
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
