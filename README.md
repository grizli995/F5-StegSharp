# F5-StegSharp - Steganography Library for .NET

F5-StegSharp is a lightweight and efficient C# library providing seamless integration of the F5 steganography algorithm in .NET applications. Supports baseline color JPEG images, each step in baseline jpeg encoding,  matrix encoding, and adaptive embedding. Ideal for secure data transmission through image files. 

## Features

- Implementation of each step in [baseline](https://medium.com/hd-pro/jpeg-formats-progressive-vs-baseline-73b3938c2339) jpeg compression. This includes:
    - Color space conversion (RGB -> YCbCr)
    - 2-D Discrete Cosine Transformation
    - Quantization
    - Run-Length Encoding
    - Huffman encoding
- Implementation of F5 steganography algorithm for hiding and extracting messages
- Matrix encoding and adaptive embedding for improved robustness
- Permutative Straddling for dispersing coefficient changes

## Installation

Install the NuGet package using the Package Manager Console:
PM> Install-Package Grizlah.Steganography.F5.Infrastructure

Or search for `F5-StegSharp` in the NuGet Package Manager in Visual Studio.

## Usage

After installing the package, register the library's services in your DI container:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddF5Services();
}
```

### Hiding a message in a JPEG image

```cs
using F5-StegSharp.Infrastructure;

// ...

var stegoService = serviceProvider.GetService<IF5Service>();
stegoService.Embed(image, "password", "Your Secret message", binaryWriter);;
```
### Extracting a message from a JPEG image

```cs
// ...
using F5Steganography;
var stegoService = serviceProvider.GetService<IF5Service>();
string message = stegoService.Extract("password", binaryReader);;

```


## License
This project is licensed under the MIT License.


## Support
If you encounter any issues or have questions, please open an issue on the GitHub repository.




