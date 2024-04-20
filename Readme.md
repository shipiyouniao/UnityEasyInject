# Unity Easy Inject

## Author

石皮幼鸟（SPYN）

## Table of Contents

* [Introduction](#introduction)
* [Installation](#installation)
* [Usage](#usage)
    * [Start the IoC Container](#1-start-the-ioc-container)
    * [Non-GameObject Component Class Object](#2-non-gameobject-component-class-object)
        * [Register Object](#21-register-object)
        * [Field Injection to Get Bean](#22-field-injection-to-get-bean)
        * [Constructor Injection to Get Bean](#23-constructor-injection-to-get-bean)
        * [Bean Name](#24-bean-name)
        * [Non-GameObject Component Class Bean Based on Liskov Substitution Principle](#25-non-gameobject-component-class-bean-based-on-liskov-substitution-principle)
    * [GameObject Object](#3-gameobject-object)
        * [Register or Only Inject GameObject Component Class](#31-register-or-only-inject-gameobject-component-class)
        * [Bean Name](#32-bean-name)
        * [Beans That Exist When the Scene Is Initialized](#33-beans-that-exist-when-the-scene-is-initialized)
        * [Register GameObjects Without Writing GameObject Component Classes](#34-register-gameobjects-without-writing-gameobject-component-classes)
        * [GameObject Component Class Bean Based on Liskov Substitution Principle](#35-gameobject-component-class-bean-based-on-liskov-substitution-principle)
* [Future Plans](#future-plans)
* [Contact Information](#contact-information)

---

## Introduction

Unity Easy Inject is a Unity dependency injection (DI) framework that can help you better manage dependencies in Unity projects, making projects easier to maintain and expand.

Using this framework, you can replace the way of manually adding public fields and then dragging and dropping injections in the Inspector for reference, or replace the way of declaring interface classes and then instantiating implementation classes, reducing module coupling and making projects easier to maintain and expand.

The usage of this framework is inspired by Spring Boot, so the usage is very similar to it.

However, since the project is still in its early stages, only class objects can be registered as Beans.

The project is developed by a junior of a college who has shifted from WEB to Unity as a newcomer, so there may be some shortcomings. Suggestions are welcome.

---

## Installation

Currently, only decompression installation is supported. Please download the project and unzip it to the Assets directory of the Unity project.

---

## Usage

### 1. Start the IoC Container

Please use `GlobalInitializer` in the `EasyInject/Controllers` directory as the startup controller and mount it on the startup object in each scene.

If the startup time of the startup controller is incorrect, causing the IoC container to not start, please set the parameter of the DefaultExecutionOrder attribute to a lower number.

```csharp
// Ensure that this script is executed first by setting a very low number
[DefaultExecutionOrder(-1000000)] 
public class GlobalInitializer : MonoBehaviour
{
    public static readonly MyIoC Instance = new();

    private void Awake()
    {
        Instance.Init();
    }
}
```

### 2. Non-GameObject Component Class Object

#### 2.1 Register Object

Non-GameObject component class objects will be registered first, which means you do not need to use `new` to create an instance of the object.

Please use attributes to mark the class as a Bean. Currently, only the `[Component]` feature is available for registration.

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

#### 2.2 Field Injection to Get Bean

You can use the `[Autowired]` attribute to inject the Bean into the field.

The injected class must also have the `[Component]` attribute, or inherit from `BeanMonoBehaviour` or `InjectableMonoBehaviour`.

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

#### 2.3 Constructor Injection to Get Bean

You can also use constructor injection to get the Bean.

***Note:*** The constructor injection method is not recommended for GameObject component classes, as the Unity engine will not be able to instantiate the class.

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

#### 2.4 Bean Name

You can use the `name` parameter of the `[Component]` attribute to specify the name of the Bean.

Then you can use the `[Autowired]` attribute to inject the Bean by name.

```csharp
[Component(name: "TestComponent4")]
public class TestComponent4
{
    public void Test()
    {
        Debug.Log("TestComponent4");
    }
}

// Inject by using the constructor
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

// Inject by using the field
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

#### 2.5 Non-GameObject Component Class Bean Based on Liskov Substitution Principle

If a class inherits another class or implements an interface, the parent class or interface will also be registered as a Bean.

If there are multiple subclasses, please be sure to specify the name in the `[Component]` of the subclass.

***However, the framework will not register according to the inheritance chain, only the parent class or interface of the implementation class will be registered. Therefore, when injecting, the type of the member variable is at most the type of the parent class. Also, please ensure that the parent class is not registered as a Bean using the `[Component]` attribute.***

***In addition, if the parent class is `Object`, it will not be registered as a Bean. Therefore, do not use `Object` as the type of the member variable for dependency injection.***

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

### 3. GameObject Object

#### 3.1 Register or Only Inject GameObject Component Class

GameObject component classes will be registered after non-GameObject component classes, which means you need to use `new` to create an instance of the object.

Please inherit from `BeanMonoBehaviour` or `InjectableMonoBehaviour` to inject the Bean.

`InjectableMonoBehaviour` means that the class will not be registered as a Bean, but can still be injected. But `BeanMonoBehaviour` will be registered as a Bean. Please choose according to your needs.

When the class is inherited from the two classes, the `Awake` method will be used for dependency injection, so a `OnAwake` method is provided, which will be called after the dependency injection is completed. Therefore, if you need to initialize, do not write the `Awake` method, but override the `OnAwake` method, and try to avoid using the `Start` method.

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

#### 3.2 Bean Name

If you need to set a name for the GameObject component class, please use the `[BeanName]` attribute.

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

#### 3.3 Beans That Exist When the Scene Is Initialized

Considering that some Beans need to exist when the scene is initialized (Including objects that are hidden by default when the scene is loaded), you can use the `[DefaultInject]` attribute, so that the Bean can be injected when the IoC container is initialized.

***Note:*** You should make sure that the object is a singleton, and that the fields to be injected are also singletons marked with that attribute.

Please pass in the scene name (without the path before the scene name) in the parameter, separated by commas between scenes.

If a class that uses this attribute is not fully injected at the beginning of the scene load, an error will be thrown. Classes that do not use this attribute will not throw an exception because they will be registered as Beans later. Please design the generation order of GameObjects reasonably.

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

#### 3.4 Register GameObjects Without Writing GameObject Component Classes

If you want to register GameObjects that do not have GameObject component classes written, you can attach the `EasyInject/Behaviours/BeanObject` script to the GameObject.

This script will register the object name as a Bean, so you need to pass in the name in the `[Autowired]` attribute when injecting the field.

***Please ensure that the object name is not duplicated, otherwise unpredictable errors may occur.***

If the object is an object that will be loaded at the beginning of the scene (including objects that are hidden by default when the scene is loaded), please check the `Is Default` property of the script in Unity.

```csharp
public class TestMonoBehaviour5 : BeanMonoBehaviour
{
    // The name here is the name of the object
    [Autowired("TestObject")]
    private BeanObject testObject;

    protected override void OnAwake()
    {
        testObject.SetActive(true);
    }
}
```

#### 3.5 GameObject Component Class Bean Based on Liskov Substitution Principle

GameObject component classes are also based on the Liskov Substitution Principle. If a class implements many interfaces, please make sure to specify the name in the `[BeanName]` of the subclass.

There will be no parent class registered as a Bean because the class is always inherited from `BeanMonoBehaviour`.

***However, the framework will not register according to the implementation chain, only the parent interface of the implementation class will be registered.***

***But if the interface is a Unity-related interface (namespace contains `UnityEngine`), the interface will be skipped.***

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

## Future Plans

1. Support for more features to make the framework more like Spring Boot while still conforming to Unity.
2. Support for more dependency injection methods, such as property injection.
3. Support for normal C# projects, not just Unity projects.
4. Register GameObjects without inheriting from `BeanMonoBehaviour` or `InjectableMonoBehaviour`, but using attributes.

---

## Contact Information

If you have any questions or suggestions, or if you would like to contribute to the project, please contact me at the following email address:

* QQ: 2960474346
* Email: 2960474346@qq.com