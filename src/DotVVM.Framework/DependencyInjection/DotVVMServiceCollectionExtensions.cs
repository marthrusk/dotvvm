using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Diagnostics;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DotvvmServiceCollectionExtensions
    {
        /// <summary>
        /// Adds essential DotVVM services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="configuration">The DotVVM configuration to use. A default one will be used if the value is <c>null</c>.</param>
        public static IServiceCollection RegisterDotVVMServices(IServiceCollection services, DotvvmConfiguration configuration = null)
        {
            services.AddOptions();
            services.TryAddSingleton<IDotvvmViewBuilder, DefaultDotvvmViewBuilder>();
            services.TryAddSingleton<IViewModelSerializer, DefaultViewModelSerializer>();
            services.TryAddSingleton<IViewModelLoader, DefaultViewModelLoader>();
            services.TryAddSingleton<IViewModelValidationMetadataProvider, AttributeViewModelValidationMetadataProvider>();
            services.TryAddSingleton<IValidationRuleTranslator, ViewModelValidationRuleTranslator>();
            services.TryAddSingleton<IViewModelValidator, ViewModelValidator>();
            services.TryAddSingleton<IViewModelSerializationMapper, ViewModelSerializationMapper>();
            services.TryAddSingleton<IViewModelParameterBinder, AttributeViewModelParameterBinder>();
            services.TryAddSingleton<IOutputRenderer, DefaultOutputRenderer>();
            services.TryAddSingleton<IDotvvmPresenter, DotvvmPresenter>();
            services.TryAddSingleton<IMarkupFileLoader, AggregateMarkupFileLoader>();
            services.TryAddSingleton<IControlBuilderFactory, DefaultControlBuilderFactory>();
            services.TryAddSingleton<IControlResolver, DefaultControlResolver>();
            services.TryAddSingleton<IControlTreeResolver, DefaultControlTreeResolver>();
            services.TryAddSingleton<IAbstractTreeBuilder, ResolvedTreeBuilder>();
            services.TryAddSingleton<IViewCompiler, DefaultViewCompiler>();
            services.TryAddSingleton<IBindingCompiler, BindingCompiler>();
            services.TryAddSingleton<IBindingExpressionBuilder, BindingExpressionBuilder>();
            services.TryAddSingleton<BindingCompilationService, BindingCompilationService>();
            services.TryAddSingleton<DataPager.CommonBindings>();
            services.TryAddSingleton<IControlUsageValidator, DefaultControlUsageValidator>();
            services.TryAddSingleton<ILocalResourceUrlManager, LocalResourceUrlManager>();
            services.TryAddSingleton<IResourceHashService, DefaultResourceHashService>();
            services.TryAddSingleton<IStopwatch, DefaultStopwatch>();
            services.TryAddSingleton<StaticCommandBindingCompiler, StaticCommandBindingCompiler>();
            services.TryAddSingleton<JavascriptTranslator, JavascriptTranslator>();
            services.TryAddSingleton<IHttpRedirectService, DefaultHttpRedirectService>();

            services.TryAddScoped<AggregateRequestTracer, AggregateRequestTracer>();
            services.TryAddScoped<ResourceManager, ResourceManager>();
            services.TryAddSingleton<Func<BindingRequiredResourceVisitor>>(s => {
               var requiredResourceControl = s.GetRequiredService<IControlResolver>().ResolveControl(new ResolvedTypeDescriptor(typeof(RequiredResource)));
               return () => new BindingRequiredResourceVisitor((ControlResolverMetadata)requiredResourceControl);
            });

            services.AddSingleton(s => configuration ?? (configuration = DotvvmConfiguration.CreateDefault(s)));
            
            services.AddDiagnosticServices();

            services.Configure<BindingCompilationOptions>(o => {
                 o.TransformerClasses.Add(ActivatorUtilities.CreateInstance<BindingPropertyResolvers>(configuration.ServiceLocator.GetServiceProvider()));
            });

            return services;
        }

        internal static IServiceCollection AddDiagnosticServices(this IServiceCollection services)
        {
            services.TryAddSingleton<DotvvmDiagnosticsConfiguration>();
            services.TryAddSingleton<IDiagnosticsInformationSender, DiagnosticsInformationSender>();

            services.AddScoped<IOutputRenderer, DiagnosticsRenderer>();
            services.AddScoped<IRequestTracer>(s =>
            {
                var config = s.GetService<DotvvmConfiguration>();
                if (config.Debug)
                {
                    var sender = s.GetService<IDiagnosticsInformationSender>();
                    return new DiagnosticsRequestTracer(sender);
                }
                else
                {
                    return new NullRequestTracer();
                }
            });
            
            return services;
        }
    }

}
