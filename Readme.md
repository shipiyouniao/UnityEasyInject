# Unity Easy Inject

## 作者

石皮幼鸟（SPYN）

## 目录

* [介绍](#介绍)
* [安装](#安装)
* [使用方法](#使用方法)
    * [启动IoC容器](#1-启动ioc容器)
    * [非游戏物体组件类对象](#2-非游戏物体组件类对象)
        * [注册对象](#21-注册对象)
        * [字段注入获取Bean](#22-字段注入获取bean)
        * [构造函数注入获取Bean](#23-构造函数注入获取bean)
        * [Bean的名字](#24-Bean的名字)
        * [基于里氏替换原则的非游戏物体组件类Bean](#25-基于里氏替换原则的非游戏物体组件类bean)
    * [游戏物体对象](#3-游戏物体对象)
        * [注册或仅注入游戏物体组件类](#31-注册或仅注入游戏物体组件类)
        * [Bean的名字](#32-Bean的名字)
        * [场景初始化时存在的Bean](#33-场景初始化时存在的bean)
        * [注册没有编写游戏物体组件类的游戏对象](#34-注册没有编写游戏物体组件类的游戏对象)
        * [基于里氏替换原则的游戏物体组件类Bean](#35-基于里氏替换原则的游戏物体组件类bean)
* [未来计划](#未来计划)
* [联系方式](#联系方式)

---

## 介绍

Unity Easy Inject是一个Unity依赖注入（DI）框架，它可以帮助你更好的管理Unity项目中的依赖关系，使得项目更加易于维护和扩展。

使用本框架，可以代替用户手动添加public字段，然后在Inspector中拖拽注入进行引用的方式，或者替代声明接口类然后实例化实现类的方式，降低模块耦合度，使得项目更加易于维护和扩展。

本框架的使用方法受SpringBoot的启发，故使用方法与其十分相似。

但由于项目目前仍在早期阶段，故只支持将类对象作为Bean进行注册。

项目由一位从WEB全栈转向Unity的大三初学者开发，故难免会有一些不足之处，欢迎大家提出宝贵意见。

---

## 安装

目前只支持解压安装。请将项目下载后解压到Unity项目的Assets目录下。

---

## 使用方法

### 1. 启动IoC容器

请把`EasyInject/Controllers`目录下的`GlobalController`作为启动控制器，挂载在每一个场景下的启动物体上。

如果启动控制器的启动时间不对，导致IoC容器没有启动，请把DefaultExecutionOrder特性的参数设置为一个更低的数字。

```csharp
// 通过设置一个非常低的数字来确保这个脚本是最先执行的
[DefaultExecutionOrder(-1000000)] 
public class GlobalInitializer : MonoBehaviour
{
    // 实例化一个IoC容器，存入静态变量中，这样就可以导致整个游戏都只有一个IoC容器
    public static readonly MyIoC Instance = new();

    private void Awake()
    {
        // 每次进入场景都初始化IoC容器
        Instance.Init();
    }
}
```

### 2. 非游戏物体组件类对象

#### 2.1 注册对象

普通对象会在场景开始时最先被注册为Bean，你不需要去亲自使用`new`关键字实例化对象。

使用特性进行Bean的注册，目前只提供`[Component]`特性进行注册。

```csharp
[Component]
public class TestComponent
{
    public void Test()
    {
        Debug.Log("TestComponent");
    }
}
```

#### 2.2 字段注入获取Bean

如果想使用字段注入，在需要使用的地方使用`[Autowired]`特性进行注入。被注入的类也必须有`[Component]`特性，或是继承了`BeanMonoBehaviour`或`InjectableMonoBehaviour`的游戏物体组件类。

```csharp
[Component]
public class TestComponent2
{
    [Autowired]
    private TestComponent testComponent;

    public void Test()
    {
        testComponent.Test();
    }
}
```

#### 2.3 构造函数注入获取Bean

如果想使用构造器注入，直接在构造函数参数中声明需要注入的对象即可。

***游戏物体组件类绝对不可以这么做，因为Unity的MonoBehaviour类是无法通过构造函数进行实例化的。***

```csharp
[Component]
public class TestComponent3
{
    private TestComponent testComponent;

    public TestComponent3(TestComponent testComponent)
    {
        this.testComponent = testComponent;
    }

    public void Test()
    {
        testComponent.Test();
    }
}
```

#### 2.4 Bean的名字

`[Component]`特性还可以接受一个字符串参数，用于指定Bean的名字，但如果使用构造函数，参数上也要使用`[Autowired]`特性传入名字。

```csharp
[Component("TestComponent4")]
public class TestComponent4
{
    public void Test()
    {
        Debug.Log("TestComponent4");
    }
}

// 使用构造函数注入
[Component]
public class TestComponent5
{
    private TestComponent4 testComponent4;

    public TestComponent5([Autowired("TestComponent4")] TestComponent4 testComponent4)
    {
        this.testComponent4 = testComponent4;
    }

    public void Test()
    {
        testComponent4.Test();
    }
}

// 使用字段注入
[Component]
public class TestComponent6
{
    [Autowired("TestComponent4")]
    private TestComponent4 testComponent4;

    public void Test()
    {
        testComponent4.Test();
    }
}
```

#### 2.5 基于里氏替换原则的非游戏物体组件类Bean

如果一个类继承了另一个类，或者实现了一个接口，那么父类或接口也会被注册为Bean。如果有多个子类，那么请务必在子类的`[Component]`当中指定名字。

***但是本框架并不会按照继承链进行注册，只会注册上一级的父类或接口。因此进行注入时，成员变量的类型至多为实现类的上一级类型。此外请保证父类并没有使用`[Component]`特性注册为Bean。***

***此外，如果父类是`Object`，则不会被注册为Bean。因此依赖注入时，不要使用`Object`作为成员变量的类型。***

```csharp
public interface ITestService
{
    void Test();
}

[Component]
public class TestService : ITestService
{
    public void Test()
    {
        Debug.Log("TestService");
    }
}

[Component]
public class TestController
{
    private ITestService testService;

    public TestController(ITestService testService)
    {
        this.testService = testService;
    }

    public void Test()
    {
        testService.Test();
    }
}
```

### 3. 游戏物体对象

#### 3.1 注册或仅注入游戏物体组件类

游戏物体组件类使用控制反转的方式是继承`BeanMonoBehaviour`或`InjectableMonoBehaviour`。

`InjectableMonoBehaviour`并不是Bean，但是可以使用字段注入。而`BeanMonoBehaviour`是Bean，请合理按照业务需求选择。

继承二者后，`Awake`生命周期钩子会被用于依赖注入，因此提供了一个`OnAwake`方法，在依赖注入完成后会被调用。因此如果需要进行初始化，请不要编写`Awake`方法，而是覆写`OnAwake`方法，并且尽可能避免使用`Start`方法。

```csharp
public class TestMonoBehaviour : BeanMonoBehaviour
{
    [Autowired]
    private TestComponent testComponent;

    protected override void OnAwake()
    {
        testComponent.Test();
    }
}
```

```csharp
public class TestMonoBehaviour2 : InjectableMonoBehaviour
{
    [Autowired]
    private TestMonoBehaviour testMonoBehaviour;
    
    protected override void OnAwake()
    {
        testMonoBehaviour.gameObject.SetActive(true);
    }
}
```

#### 3.2 Bean的名字

如果您需要给游戏物体组件类设置名称，请使用`[BeanName]`特性。

```csharp
[BeanName("TestMonoBehaviour3")]
public class TestMonoBehaviour3 : BeanMonoBehaviour
{
    [Autowired]
    private TestComponent testComponent;

    protected override void OnAwake()
    {
        testComponent.Test();
    }
}
```

#### 3.3 场景初始化时存在的Bean

考虑到生命周期的问题，在场景初始化时便存在的游戏物体组件单例（包括场景加载时默认隐藏的物体），可以使用`[DefaultInject]`特性，这样可以在IoC容器初始化时就注入。

***但如果这样做的话，必须确保这个类是单例，并且需要被注入的字段也是标记了该特性的单例，否则会导致不可预知的错误。***

请在参数中传入场景名称（不要带场景名之前的路径），场景之间用逗号隔开。

使用了该特性的类如果在场景加载一开始没有被完成全部字段的注入，则会产生错误抛出异常。没有使用该特性的类，由于会在之后的过程中被注册为Bean，因此不会抛出异常。请针对游戏物体生成顺序进行合理的设计。

```csharp
[DefaultInject("SampleScene", "SampleScene2")]
public class TestMonoBehaviour4 : BeanMonoBehaviour
{
    [Autowired]
    private TestComponent testComponent;

    protected override void OnAwake()
    {
        testComponent.Test();
    }
}
```

#### 3.4 注册没有编写游戏物体组件类的游戏对象

如果您想要把没有编写游戏物体组件类的游戏对象注册为Bean，可以在物体上挂载`EasyInject/Behaviours/BeanObject`脚本。

这个脚本会把物体名称作为Name注册为Bean，因此在字段注入时，需要在`[Autowired]`特性中传入名字。

***请保证物体名称不会重复，否则会导致不可预知的错误。***

如果物体是一个初始就会被加载的物体（包括场景加载时默认隐藏的物体），请在Unity当中把脚本的`Is Default`属性勾选上。

```csharp
public class TestMonoBehaviour5 : BeanMonoBehaviour
{
    // 这里的名字是物体的名字
    [Autowired("TestObject")]
    private BeanObject testObject;

    protected override void OnAwake()
    {
        testObject.SetActive(true);
    }
}
```

#### 3.5 基于里氏替换原则的游戏物体组件类Bean

游戏物体组件类也可以基于里氏替换原则进行注册。如果有多个子实现类，那么请务必在子类的`[BeanName]`当中指定名字。

由于组件类是必须继承`BeanMonoBehaviour`的，因此这里不会出现父类被注册为Bean的情况。

***但是本框架并不会按照继承链进行注册，只会注册上一级的接口。因此进行注入时，成员变量的类型至多为实现类的上一级类型。***

***但是如果接口是Unity相关的接口（命名空间包含`UnityEngine`），则这个接口会被略过。***

```csharp
public interface ISay
{
    void say();
}

[BeanName("TestMonoBehaviour6")]
public class TestMonoBehaviour6 : BeanMonoBehaviour, ISay
{
    public void say()
    {
        Debug.Log("TestMonoBehaviour6");
    }
}

[BeanName("TestMonoBehaviour7")]
public class TestMonoBehaviour7 : BeanMonoBehaviour, ISay
{
    public void say()
    {
        Debug.Log("TestMonoBehaviour7");
    }
}

public class TestMonoBehaviour8 : InjectableMonoBehaviour
{
    [Autowired("TestMonoBehaviour6")]
    private ISay testMonoBehaviour6;
    
    [Autowired("TestMonoBehaviour7")]
    private ISay testMonoBehaviour7;
    
    protected override void OnAwake()
    {
        testMonoBehaviour6.say();
        testMonoBehaviour7.say();
    }
}
```

---

## 未来计划

* 支持更多的特性，让框架在符合Unity的同时更加逼近SpringBoot
* 支持更多的依赖注入方式，如属性注入
* 适应非Unity项目的普通C#项目
* 仅使用特性进行对游戏物体组件类的注册，不需要继承其他类

---

## 联系方式

如果您有任何问题或建议，或是想要参与代码贡献，请联系我

* QQ: 2960474346
* 邮箱: 2960474346@qq.com