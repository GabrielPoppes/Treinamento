using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treinamento
{
    public class Agendar : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)); // Estamos gerando o Context (linha Padrão)

            /*
             * Nas duas linhas abaixo estamos criando o service. Isso porquê, quando eu crio o Plugin, eu consigo manipular a entidade. Portanto, quando crio uma ocorrência
             * eu posso já começar a manipular os dados da ocorrência, mas, eu preciso do service, porquê a maioria dos Plugins permite que eu manipule dados de outras entidades
             * então preciso do service para chamar os dados para fazer por exemplo: Retriever (que pega dados de outra entidade), Create (para criar uma entidade).
             * Além disso, consigo usar o Services para obter dados de uma mesma entidade, porém, que não entram no contexto. Por exemplo, o Update! No Update o Plugin pode rodar
             * referenciar outros campos que estão fora do nosso contexto.
             */
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            /*
             * Na condição abaixo, estamos validando se as configurações estão batendo com as mesmas que colocamos no Registration Tool
             * Imagine que nós vamos ter um Plugin para rodar em cada entidade, aí se não tiver essa validação abaixo, ele vai rodar em todas!
             * Veja abaixo, que defino a entidade "incident"
             */
            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "opportunity") // No caso alteramos para oportunidade
            {
                /*
                 * Na condição abaixo, vai chegar se no context, em "Keys", ele recebe o valor  Target!
                 * E após, verifica se o Target é uma Entidade. No Create que estamos usando, sempre será uma entidade, mas em outros casos não
                 * (Exemplo: incluindo um registro, não vai retornar uma entidade completa - e sim, trazer o ID e nome lógico da entidade
                 */
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"]; // Jogando para a variável targetIncident
                    String compromise = string.Empty;
                    DateTime dataAtual = DateTime.Now;
                    TimeSpan begin_compromise = new TimeSpan(9, 0, 0);
                    TimeSpan end_compromise = new TimeSpan(11, 0, 0);

                    if (entity.Contains("name"))
                    {
                        compromise = entity["name"].ToString();
                    }

                    /*
                     *  ======= OBTENDO O PROPRIETÁRIO DA OCORRÊNCIA =======
                     */
                    EntityReference proprietario = new EntityReference(); // criando uma entidade de referência (por enquanto vazia)
                    try
                    {
                        // Método Retrieve (pega os dados), pegando informações do targetIncident, e informando quais dados queremos (colocar ownerid, mas poderia ser email etc...) 
                        Entity compromise_entity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ownerid"));
                        proprietario = (EntityReference)compromise_entity["ownerid"]; //  atribuindo o Ownerid (do user que criou a ocorrência) a variável do tipo EntityReference que criamos acima
                    }
                    catch (Exception ex)
                    {
                        // A exceção de um Plugin, é do tipo InvalidPluginExecutionException
                        throw new InvalidPluginExecutionException("Erro ao localizar proprietário: " + ex.Message);
                    }

                    /*
                     * ====== GERANDO A TAREFA =======
                     * Nos campos abaixo, estamos simplesmente atribuindo os registros
                     */

                    do
                    {
                        dataAtual = dataAtual.AddDays(1);
                        dataAtual = dataAtual.Date + begin_compromise;
                    } while (dataAtual.DayOfWeek != DayOfWeek.Monday);

                    Entity novaTarefa = new Entity("appointment");
                    novaTarefa["subject"] = "Fornecer agendamento para a ocorrência " + compromise; // subject = titulo
                    novaTarefa["regardingobjectid"] = new EntityReference(entity.LogicalName, entity.Id);
                    novaTarefa["ownerid"] = (EntityReference)proprietario;
                    novaTarefa["scheduledstart"] = dataAtual; // Data inicial da agendamento
                    novaTarefa["scheduledend"] = dataAtual.Date + end_compromise; // Data final

                    try
                    {
                        service.Create(novaTarefa); // Criando a nova tarefa
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidPluginExecutionException("Erro ao gerar tarefa: " + ex.Message); // Exceção caso dê erro na hora de criar a tarefa
                    }

                }
            }
        }
    }
}
