# SignDocs App
SignDocs is a mock version of a real document-signing application. It simulates signing documents using a private key and password (provided once per session).

## How it works
A user needs to choose their private key and then provide their password.

![AppUI1](https://github.com/user-attachments/assets/3b55724f-833f-401c-b96f-e97702b0d2a6)
![AppUI2](https://github.com/user-attachments/assets/eea4f684-40ff-477d-9c4b-2e70c0128c32)

Each document is signed twice:
1. **Attached Signature** – the first signature is embedded in the PDF file.
2. **QR Signature** – the second signature is used to generate two QR codes.

Documents have unique names based on:
* Document type
* Number
* Date

The naming convention forms a directory structure. For example, a document named `10001_1_20250126.pdf` will be stored as 
`/2025/01/10001/10001_1_20250126/10001_1_20250126.pdf`.

Multiple users can sign the same document. When the final user signs, the app appends a new page containing all users' QR codes, as shown below:

![PdfPage](https://github.com/user-attachments/assets/412b946a-208f-431d-b022-fe8cc1215089)


## Installation
* This is a **Windows Forms** app which targets **.NET Framework 4.8**.
* To start the app you just need to build it and run **SignDocs.exe**.
* You can pass any credentials.
* In order to simulate documents signing you need some data from the **Data** folder. The predefined paths can be found in `Form.cs`.
* To simulate a different user, change the `Name` property in the `MockCertificate` class.

## Dependencies
The app uses these libraries:
* [PDFsharp](https://www.nuget.org/packages/PDFsharp/) - a library for creating and processing PDF documents.
* [QRCoder](https://www.nuget.org/packages/QRCoder) - a library for generating QR codes.

The original application also uses a government-provided library for interacting with private keys. Due to legal and technical constraints, this library is excluded. A mock version `MockCryptoService` has been implemented to simulate its behavior.
