using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DidacticalEnigma.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton(typeof(IRichFormattingHtmlRenderer), provider => new RichFormattingHtmlRenderer());
            services.AddSingleton(provider =>
            {
                var dataDir = @"D:\DidacticalEnigma-Data";
                var get = Context.Configure(dataDir);
                var wordDataSources = new List<IDataSource>
                {
                    //new CharacterDataSource(get.Get<IKanjiProperties>(), get.Get<IKanaProperties>()),
                    //new CharacterStrokeOrderDataSource(),
                    new JMDictDataSource(get.Get<JMDictLookup>(), get.Get<IKanaProperties>()),
                    new JNeDictDataSource(get.Get<Jnedict>()),
                    new VerbConjugationDataSource(get.Get<JMDictLookup>()),
                    new WordFrequencyRatingDataSource(get.Get<FrequencyList>()),
                    new PartialExpressionJMDictDataSource(get.Get<IdiomDetector>()),
                    new JGramDataSource(get.Get<IJGramLookup>()),
                    new CustomNotesDataSource(Path.Combine(dataDir, "custom", "custom_notes.txt")),
                    new TanakaCorpusFastDataSource(get.Get<Corpus>())
                }.Concat(get.Get<EpwingDictionaries>().Dictionaries.Select(dict => new EpwingDataSource(dict, get.Get<IKanaProperties>())));
                var overallDataSources = new List<IDataSource>
                {
                    new RomajiDataSource(get.Get<IRomaji>()),
                    new AutoGlosserDataSource(get.Get<IAutoGlosser>()),
                };
                return new Context(wordDataSources, overallDataSources, provider.GetService<IRichFormattingHtmlRenderer>(), get.Get<ISentenceParser>());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
