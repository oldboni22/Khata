using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

namespace TopicService.Tests.AutoData;

[AttributeUsage(AttributeTargets.Method)]
public class AutoDomainDataAttribute() : AutoDataAttribute(() =>
    new Fixture()
        .Customize(new AutoNSubstituteCustomization())
        .Customize(new Customizations.ValidInputsCustomization()));
