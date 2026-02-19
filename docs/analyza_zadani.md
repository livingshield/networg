# 📋 Analýza zadání – NETWORG Power Apps Candidate Review Assignment

## Přehled

Jedná se o úkol v rámci přijímacího řízení do firmy **NETWORG**. Cílem je vytvořit **model-driven aplikaci v Power Apps** pro fiktivní stavební firmu **ConstructSafe Inc.**, která potřebuje systém na sledování a řízení **neshod (non-conformities)** na stavbách.

> [!IMPORTANT]
> Zadání zdůrazňuje, že není nutné aplikaci zcela dokončit – důležitější je ukázat **přístup, myšlenkový proces a schopnost se učit**.

---

## 🏗️ Kontext zadání

**ConstructSafe Inc.** potřebuje aplikaci, která umožní:

- Pracovníkům a manažerům **hlásit problémy** (neshody)
- **Přiřazovat nápravná opatření** (corrective actions)
- **Zajistit včasné notifikace** odpovědným manažerům

---

## 📌 Požadované User Stories (hlavní funkce)

| # | User Story | Popis |
|---|-----------|-------|
| 1 | **Report Non-Conformity** | Uživatelé mohou hlásit různé typy neshod, formulář se dynamicky mění podle zvoleného typu |
| 2 | **Attach Evidence** | Možnost přiložit fotografie a dokumenty ke každému hlášení |
| 3 | **Corrective Actions** | Vytváření a přiřazování nápravných opatření k neshodám |
| 4 | **Assign Managers** | Přiřazení odpovědného manažera ke každé neshodě |
| 5 | **Notifications** | Automatické notifikace manažerům při nové nebo aktualizované neshodě |

---

## ⭐ Bonus Points (vyšší hodnocení)

- **JavaScript/TypeScript** pro podmíněné zobrazování polí formuláře + diskuze o rozdílu mezi business rules vs. custom skripty
- **Power Automate flow** pro generování PDF souhrnu hlášení
- **Git source control** pro verzování řešení

---

## 🎯 Optional Tasks (pokročilé)

Tyto úkoly testují hlubší znalost Power Platform:

| # | Úkol |
|---|------|
| 1 | Přepsat výchozí onclick chování homepage gridu |
| 2 | Canvas aplikace pro Corrective Actions (nahrávání souborů/obrázků) |
| 3 | Autonumbering plugin (automatické číslování) |
| 4 | BPF proces s přepsaným chováním tlačítka "next stage" |
| 5 | HTTP-triggered Power Automate flow pro vytvoření Non-Conformity záznamu |
| 6 | Custom connector v Power Automate |
| 7 | Synchronní logika pro kopírování neshody včetně child záznamů (corrective actions, errors) |

---

## 📊 Hodnotící kritéria (seřazeno dle váhy)

1. **Schopnost splnit zadání** – nejdůležitější
2. **Schopnost nastavit prerequisites** – dev environment, nástroje
3. **Znalost TypeScript, Model Driven Apps, Power Automate**
4. **Principy softwarového vývoje / design patterns**
5. **Míra potřebné asistence od NETWORGu** – čím méně, tím lepší hodnocení

---

## 🛠️ Požadované nástroje a prostředí

| Nástroj | Popis |
|---------|-------|
| **Power Apps Developer Environment** | Test tenant + dev environment |
| **Microsoft Power Platform CLI** | Pro pokročilé úkoly |
| **Visual Studio Code** | Vývojové prostředí |
| **TypeScript** | Pro custom scripty |

---

## 📦 Výstup – co se odevzdává

Strukturovaná **prezentace** obsahující:

- Předchozí znalosti a dovednosti relevantní k úkolu
- Výzvy, se kterými jste se potýkali
- Zvažované varianty implementace + rozhodnutí
- **Živá demonstrace** vytvořeného řešení
- Klíčové poznatky a zkušenosti z realizace

---

## ⏰ Časový rámec

> Zadání doporučuje **2 týdny** na dokončení.

---

## 💡 Navrhovaný postup řešení

### Fáze 1: Příprava prostředí

- Vytvoření Power Apps Developer tenant/environment
- Instalace Power Platform CLI, VS Code, TypeScript

### Fáze 2: Datový model (Dataverse)

- Tabulka **Non-Conformity** (typ, popis, stav, priorita, přiřazený manažer, datum...)
- Tabulka **Corrective Action** (vztah N:1 k Non-Conformity)
- Tabulka **Evidence/Attachment** (fotky, dokumenty)
- Volitelně: choice/option set pro typy neshod

### Fáze 3: Model-Driven App

- Formuláře s dynamickými poli dle typu neshody
- Views pro přehledy neshod
- Dashboardy pro manažery

### Fáze 4: Automatizace (Power Automate)

- Flow pro notifikace při vytvoření/aktualizaci neshody
- Bonus: Flow pro PDF generování

### Fáze 5: Bonus + Optional

- TypeScript skript pro podmíněná pole
- Autonumbering plugin
- BPF, custom connector atd.

---

## ❓ Otázky k diskuzi

Před započetím implementace bychom měli prodiskutovat:

1. **Máš již přístup k Power Apps Developer prostředí?** (test tenant)
2. **Jaká je tvá zkušenost s Power Apps / Power Platform?** – abych mohl přizpůsobit úroveň vedení
3. **Na které části se chceš zaměřit nejvíce?** – hlavní funkce, bonus, nebo optional tasks?
4. **Máš nainstalované VS Code a Power Platform CLI?**
5. **Chceš začít datovým modelem, nebo rovnou UI?**
