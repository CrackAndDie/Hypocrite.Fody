<p align="center">
  <a href="https://robocadsim.readthedocs.io/en/latest/index.html">
    <img src="https://raw.githubusercontent.com/CADindustries/container/main/logos/baguette-custom.png" alt="Abdrakov.Solutions logo" width="256" height="256">
  </a>
</p>
<h1 align="center">Abdrakov.Fody</h1>  

[![Nuget](https://img.shields.io/nuget/v/Abdrakov.Fody.svg)](http://nuget.org/packages/Abdrakov.Fody)
[![Nuget](https://img.shields.io/nuget/dt/Abdrakov.Fody.svg)](http://nuget.org/packages/Abdrakov.Fody)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/CrackAndDie/Abdrakov.Fody/blob/main/LICENSE)

<h2>About:</h2>  

A package that allows You to use ```[Notify]``` attribute in Your code with *Abdrakov.Solutions* library. 

<h2>Download:</h2>  

<h3>.NET CLI:</h3>  

```dotnet add package Abdrakov.Fody```

<h3>Package Reference:</h3>  

```<PackageReference Include="Abdrakov.Fody" Version="*" />```   

<h2>Getting started:</h2>  

You can use ```[Nofity]``` attribute on any property You have in Your class but the class has to be inherited from *BindableObject*.  
To start using this library You should add a package reference to the library in Your project like:
```xml
<PackageReference Include="Abdrakov.Fody" Version="*">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; compile; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```
And create a *FodyWeavers.xml* file in Your project root with following configuration (if You already have one - ```<Abdrakov />``` could be just added there):
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Abdrakov />
</Weavers>
```

<h2>Powered by:</h2>  

- *Abdrakov.Fody*' logo - [Material Design Icons](https://materialdesignicons.com/)
- *Abdrakov.Fody* is a rewritten part of [ReactiveUI](https://github.com/reactiveui/ReactiveUI)
