# Dokumentacja Backendu CarRental

## Spis Treści
1. [Endpointy API](#endpointy-api)
2. [Baza Danych](#baza-danych)
3. [Relacje](#relacje)

## Endpointy API

### AuthController (Uwierzytelnianie)
Kontroler odpowiedzialny za rejestrację, logowanie i zarządzanie tokenami użytkowników.

| Metoda | Endpoint | Opis | Wymagane Uprawnienia |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/Auth/register` | Rejestracja nowego użytkownika. | Brak |
| `POST` | `/api/Auth/login` | Logowanie użytkownika. Zwraca token JWT. | Brak |
| `POST` | `/api/Auth/refresh-token` | Odświeżenie tokena dostępu (Refresh Token). | Brak |
| `GET` | `/api/Auth/me` | Pobranie danych aktualnie zalogowanego użytkownika. | Zalogowany |
| `GET` | `/api/Auth/admin` | Testowy endpoint dostępny tylko dla administratora. | Admin |

### CarController (Samochody)
Kontroler służący do przeglądania dostępnych samochodów.

| Metoda | Endpoint | Opis | Wymagane Uprawnienia |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/Car` | Pobranie listy wszystkich samochodów. | Brak |
| `GET` | `/api/Car/{id}` | Pobranie szczegółów konkretnego samochodu po ID. | Brak |

### ReservationController (Rezerwacje)
Kontroler do zarządzania rezerwacjami.

| Metoda | Endpoint | Opis | Wymagane Uprawnienia |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/Reservation` | Utworzenie nowej rezerwacji. | Zalogowany |
| `DELETE` | `/api/Reservation/{id}` | Anulowanie własnej rezerwacji. | Zalogowany |
| `GET` | `/api/Reservation/admin/all` | Pobranie wszystkich rezerwacji w systemie. | Admin |
| `DELETE` | `/api/Reservation/admin/{id}` | Anulowanie dowolnej rezerwacji przez administratora. | Admin |

---

## Baza Danych

### Tabela: `Users` (Użytkownicy)
Przechowuje informacje o użytkownikach systemu.

| Kolumna | Typ | Opis |
| :--- | :--- | :--- |
| `Id` | `Guid` | Unikalny identyfikator użytkownika (Klucz Główny). |
| `Email` | `string` | Adres email użytkownika. |
| `PasswordHash` | `string` | Zahaszowane hasło. |
| `Role` | `string` | Rola użytkownika (np. "Admin", "User"). |
| `RefreshToken` | `string?` | Token odświeżający. |
| `RefreshTokenExpiryTime` | `DateTime?` | Data wygaśnięcia tokena odświeżającego. |

### Tabela: `Cars` (Samochody)
Przechowuje informacje o flocie samochodowej.

| Kolumna | Typ | Opis |
| :--- | :--- | :--- |
| `Id` | `int` | Unikalny identyfikator samochodu (Klucz Główny). |
| `Brand` | `string` | Marka samochodu. |
| `Model` | `string` | Model samochodu. |
| `Year` | `int` | Rok produkcji. |
| `PricePerDay` | `decimal` | Cena za dzień wynajmu. |
| `Description` | `string?` | Opis samochodu. |
| `ImageUrl` | `string?` | URL do zdjęcia samochodu. |

### Tabela: `Reservations` (Rezerwacje)
Przechowuje informacje o rezerwacjach dokonanych przez użytkowników.

| Kolumna | Typ | Opis |
| :--- | :--- | :--- |
| `Id` | `int` | Unikalny identyfikator rezerwacji (Klucz Główny). |
| `CarId` | `int` | ID rezerwowanego samochodu (Klucz Obcy -> Cars). |
| `UserId` | `Guid` | ID użytkownika dokonującego rezerwacji (Klucz Obcy -> Users). |
| `StartDateTime` | `DateTime` | Data i czas rozpoczęcia rezerwacji. |
| `EndDateTime` | `DateTime` | Data i czas zakończenia rezerwacji. |
| `TotalPrice` | `decimal` | Całkowity koszt rezerwacji. |
| `CreatedAt` | `DateTime` | Data utworzenia rezerwacji. |

---

## Relacje

1.  **Users ↔ Reservations**: Relacja jeden-do-wielu (One-to-Many).
    -   Jeden użytkownik (`User`) może mieć wiele rezerwacji (`Reservations`).
    -   Każda rezerwacja jest przypisana do jednego użytkownika.

2.  **Cars ↔ Reservations**: Relacja jeden-do-wielu (One-to-Many).
    -   Jeden samochód (`Car`) może być przedmiotem wielu rezerwacji (w różnym czasie).
    -   Każda rezerwacja dotyczy jednego konkretnego samochodu.
