using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace LoM_Calculator
{
    class Program
    {
        private static List<Heroi> heroOptions;
        private static List<Task> tarefas;
        private static long count;
        static void Main(string[] args)
        {
            Console.WriteLine("Legend of Mana Levels Calculator!");
            Console.WriteLine("This was developed to find the best status-wise leveling solution for Legend of Mana.");
            Console.WriteLine("There is 11 weapon types and 98 levels to be grinded, so this was made to find the best main character. ");
            //Console.WriteLine("Type the level per weapon limit (for faster results), the recomendation is 15");
            //string valor = Console.ReadLine();
            Console.WriteLine("This WILL take a while, but when finished it will generate a json file with everything you need to know. ");
            Console.WriteLine("");

            tarefas = new List<Task>();
            heroOptions = new List<Heroi>();
            count = 0;
            
            // o primeiro não tem regra calculada, o ultimo vira parametro direto
            int[] regras = new int[Valores.tiposArmas - 2];

            #region loop
            Task l = loop();

            /*
            for (var a = 0; a <= Valores.maxLoop; a++)
            {
                regras[0] = Valores.maxLvl - a;
                for (int b = 0; b < regras[0] && b <= Valores.maxLoop; b++)
                {
                    regras[1] = regras[0] - b;
                    for (int c = 0; c < regras[1] && c <= Valores.maxLoop; c++)
                    {
                        regras[2] = regras[1] - c;
                        for (int d = 0; d < regras[2] && d <= Valores.maxLoop; d++)
                        {
                            regras[3] = regras[2] - d;
                            for (int e = 0; e < regras[3] && e <= Valores.maxLoop; e++)
                            {
                                regras[4] = regras[3] - e;
                                for (int f = 0; f < regras[4] && f <= Valores.maxLoop; f++)
                                {
                                    regras[5] = regras[4] - f;
                                    for (int g = 0; g < regras[5] && g <= Valores.maxLoop; g++)
                                    {
                                        regras[6] = regras[5] - g;
                                        for (int h = 0; h < regras[6] && h <= Valores.maxLoop; h++)
                                        {
                                            regras[7] = regras[6] - h;
                                            for (int i = 0; i < regras[7] && i <= Valores.maxLoop; i++)
                                            {
                                                regras[8] = regras[7] - i;
                                                // for (int j = 0; j < regras[8] && j <= 20; j++)
                                                for (int j = 0; j < regras[8] && j <= Valores.maxLoop; j++)
                                                {
                                                    Task x = listaHero(new List<int> { a, b, c, d, e, f, g, h, i, j });
                                                    count++;
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            */
            #endregion

            //aguarda termino
            l.Wait();
            heroOptions = heroOptions.OrderByDescending(hero => hero.overall).ToList();

            //DO IT
            string filename = "LoM_" + heroOptions.Count + ".json";
            File.WriteAllText(filename, JsonConvert.SerializeObject(heroOptions));
            Console.WriteLine("!");
            Console.WriteLine("File Complete! " + filename);
            Console.ReadLine();
        }

        static async Task loop()
        {
            //inicia o loop
            //a/b/c/d/e/f/g/h/i/j
            List<int> entradaIniciais = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            await loop(entradaIniciais);
        }
        static async Task<bool> loop(List<int> entradasLoop)
        {
            List<int> listaLoop = entradasLoop;
            while (listaLoop.First() <= Valores.maxLvl)
            {
                //garantindo que o ultimo está zerado para a soma
                int ultimo = listaLoop.Count() - 1;
                listaLoop[ultimo] = 0;

                //limite do loop sendo o nivel maximo
                int soma = listaLoop.Sum();
                int limiteLoop = Valores.maxLvl - soma;

                for (int i = limiteLoop; i >= 0; i--)
                {
                    listaLoop[ultimo] = i;
                    tarefas.Add(listaHero(listaLoop));
                }

                //adiciona um valor nas outras casas
                int indice = -1;
                int somaNova;
                bool novoIndice = false;
                for (int i = 0; i < ultimo; i++)
                {
                    if (novoIndice)
                    {
                        listaLoop[i] = 0;
                    }
                    else if (listaLoop[i] > 0)
                    {
                        indice = i;
                        listaLoop[indice] = listaLoop[indice] + 1;

                        //passa para o proximo indice
                        somaNova = listaLoop.Sum();
                        if (somaNova > Valores.maxLvl)
                        {
                            novoIndice = true;
                            if (i > 0)
                            {
                                listaLoop[indice] = 0;
                                indice--;
                                listaLoop[indice] = 1;
                            }

                            //aguarda execução de todas as tarefas para não desencadear stack overflow
                            await Task.WhenAll(tarefas.ToArray());
                            //tarefas.ForEach(t => t.Wait());
                            tarefas = new List<Task>();
                        }
                    }
                }
                if(soma == 0)
                {
                    listaLoop[ultimo - 1] = 1;
                }
            }
            return true;
        }

        static int[] quantidadesRes(List<int> quantidades)
        {
            List<int> novaLista = quantidades.ToArray().ToList();
            int max = Valores.tiposArmas - 1;

            for (int j = novaLista.Count; j < max; j++)
            {
                novaLista.Add(0);
            }

            int soma = novaLista.Sum();
            novaLista.Add(Valores.maxLvl - soma);
            novaLista.Reverse();

            return novaLista.ToArray();
        }

        static async Task listaHero(List<int> quantidades)
        {
            await NewHero(quantidadesRes(quantidades));
        }

        private static async Task NewHero(int[] quantidades)
        {
            Heroi novo = Valores.novoHeroi(quantidades);

            if (novo.overall >= Valores.minOverall && novo.nivel == Valores.maxStatus)
            {
                heroOptions.Add(novo);
                Console.Write("+");
            }
        }
    }


    #region ENUMERADORES
    public enum Armas
    {
        Knife,
        Sword1H,
        Axe1H,
        Sword2H,
        Axe2H,
        Hammer,
        Spear,
        Staff,
        Glove,
        Flail,
        Bow
    }

    public enum Status
    {
        POW,
        SKL,
        DEF,
        MAG,
        HP,
        SPR,
        CHM
    }
    #endregion

    #region READ ONLY
    public static class Valores
    {
        public static int tiposArmas = 11;
        public static int tiposStatus = 7;
        public static int statusInicial = 5;
        public static double porStatus = 0.25;
        public static int maxLoop = 20;
        public static int maxStatus = 99;
        public static int maxLvl = 98;
        public static int minOverall = 618;

        //ordem para calculo
        public static string[] ordemDeInteresse = new string[]
        {
            Armas.Sword1H.ToString(),
            Armas.Staff.ToString(),
            Armas.Hammer.ToString(),
            Armas.Bow.ToString(),
            Armas.Spear.ToString(),
            Armas.Knife.ToString(),
            Armas.Axe1H.ToString(),
            Armas.Axe2H.ToString(),
            Armas.Flail.ToString(),
            Armas.Glove.ToString(),
            Armas.Sword2H.ToString()
        };

        //status das armas por nivel, onde x*0,25
        public static List<Arma> armas = new List<Arma>()
        {
            new Arma(Armas.Knife, 3, 5, 3, 3, 2, 4, 4),
            new Arma(Armas.Sword1H, 4, 4, 4, 3, 3, 3, 3),
            new Arma(Armas.Axe1H, 4, 3, 4, 3, 3, 4, 2),
            new Arma(Armas.Sword2H, 5, 4, 3, 3, 2, 3, 2),
            new Arma(Armas.Axe2H, 5, 3, 5, 3, 2, 3, 2),
            new Arma(Armas.Hammer, 7, 3, 2, 3, 3, 3, 3),
            new Arma(Armas.Spear, 3, 5, 4, 3, 2, 3, 4),
            new Arma(Armas.Staff, 2, 3, 3, 5, 2, 3, 5),
            new Arma(Armas.Glove, 5, 3, 3, 2, 4, 3, 3),
            new Arma(Armas.Flail, 3, 5, 2, 3, 2, 3, 5),
            new Arma(Armas.Bow, 2, 7, 3, 3, 2, 3, 4)
        };

        public static Heroi novoHeroi(int[] qtd)
        {
            Heroi novo = new Heroi();

            for (int i = 0; i < qtd.Length; i++)
                novo.addStatus(armas.Find(a => a.Nome == ordemDeInteresse[i]), qtd[i]);

            novo.updateStatus();
            return novo;
        }

    }
    #endregion

    #region PARAMETRIZAÇÃO
    public class Arma
    {
        public string Nome;
        public List<Atributo> Atributos;

        public Arma(Armas nome, int pow, int skl, int def, int mag, int hp, int spr, int chm)
        {
            Nome = nome.ToString();
            Atributos = new List<Atributo>();

            Atributos.Add(new Atributo(Status.POW, pow));
            Atributos.Add(new Atributo(Status.SKL, skl));
            Atributos.Add(new Atributo(Status.DEF, def));
            Atributos.Add(new Atributo(Status.MAG, mag));
            Atributos.Add(new Atributo(Status.HP, hp));
            Atributos.Add(new Atributo(Status.SPR, spr));
            Atributos.Add(new Atributo(Status.CHM, chm));
        }
    }

    public class Atributo
    {
        public string Nome;
        public double Valor;

        public Atributo(Status st)
        {
            Nome = st.ToString();
            Valor = Valores.statusInicial;
        }
        public Atributo(Status st, double qtd)
        {
            Nome = st.ToString();
            Valor = Valores.porStatus * qtd;
        }
        public Atributo(string nome, double valor)
        {
            Nome = nome;
            Valor = valor;
        }
    }
    #endregion

    #region OBJETO DE RETORNO
    public class Heroi
    {
        public int nivel;
        public int overall;
        public List<Atributo> niveis;
        public List<Atributo> statusReal;
        public List<Atributo> statusCalculado;

        public Heroi()
        {
            nivel = 1;

            niveis = new List<Atributo>();
            statusReal = new List<Atributo>();
            statusCalculado = new List<Atributo>();

            for (var j = 0; j < Valores.tiposStatus; j++)
            {
                statusReal.Add(new Atributo((Status)j));
                statusCalculado.Add(new Atributo((Status)j));
            }

            updateStatus();
        }

        public void addStatus(Arma arma, int qtd)
        {
            if (qtd > 0)
            {
                niveis.Add(new Atributo(arma.Nome, qtd));
                arma.Atributos.ForEach(obj =>
                {
                    statusCalculado.Find(st => st.Nome == obj.Nome).Valor += (obj.Valor * qtd);
                });

                nivel += qtd;
            }
        }

        public void updateStatus()
        {
            statusCalculado.ForEach(stC =>
            {
                double valorCalc = (stC.Valor >= Valores.maxStatus) ? Valores.maxStatus : Math.Floor(stC.Valor);
                statusReal.Find(st => st.Nome == stC.Nome).Valor = valorCalc;
            });

            overall = (int)statusReal.Sum(st => st.Valor);
        }
    }


    #endregion
}
