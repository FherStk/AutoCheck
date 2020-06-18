--
-- PostgreSQL database dump
--

-- Dumped from database version 10.11
-- Dumped by pg_dump version 10.11

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE ONLY test.work_history DROP CONSTRAINT work_history_work_fk;
ALTER TABLE ONLY test.work_history DROP CONSTRAINT work_history_departments_fk;
ALTER TABLE ONLY test.localizations DROP CONSTRAINT localizations_countries_fk;
ALTER TABLE ONLY test.employees DROP CONSTRAINT employees_work_fk;
ALTER TABLE ONLY test.departments DROP CONSTRAINT departments_localizations_fk;
ALTER TABLE ONLY test.departments DROP CONSTRAINT departments_cap_fk;
ALTER TABLE ONLY test.countries DROP CONSTRAINT countries_regions_fk;
DROP RULE update_pb ON test.programmers_bosses;
DROP RULE ins_programmer_default ON test.programmers;
DROP RULE ins_programmer ON test.programmers;
DROP RULE delete_pb ON test.programmers_bosses;
ALTER TABLE ONLY test.work DROP CONSTRAINT work_pk;
ALTER TABLE ONLY test.work_history DROP CONSTRAINT work_history_pk;
ALTER TABLE ONLY test.regions DROP CONSTRAINT regions_pk;
ALTER TABLE ONLY test.localizations DROP CONSTRAINT localizations_pk;
ALTER TABLE ONLY test.employees DROP CONSTRAINT employees_pk;
ALTER TABLE ONLY test.departments DROP CONSTRAINT departament_pk;
ALTER TABLE ONLY test.countries DROP CONSTRAINT countries_pk;
DROP TABLE test.work_history;
DROP TABLE test.regions;
DROP VIEW test.programmers_bosses;
DROP TABLE test.work;
DROP VIEW test.programmers;
DROP TABLE test.localizations;
DROP TABLE test.employees;
DROP TABLE test.departments;
DROP TABLE test.countries;
DROP TABLE test.categories;
DROP EXTENSION plpgsql;
DROP SCHEMA test;
DROP SCHEMA public;
--
-- Name: public; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA public;


ALTER SCHEMA public OWNER TO postgres;

--
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: postgres
--

COMMENT ON SCHEMA public IS 'standard public schema';


--
-- Name: test; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA test;


ALTER SCHEMA test OWNER TO postgres;

--
-- Name: SCHEMA test; Type: COMMENT; Schema: -; Owner: postgres
--

COMMENT ON SCHEMA test IS 'standard test schema';


--
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: categories; Type: TABLE; Schema: test; Owner: postgres
--

CREATE TABLE test.categories (
    category character varying(3),
    lower_salary smallint,
    higher_salary integer
);


ALTER TABLE test.categories OWNER TO postgres;

--
-- Name: countries; Type: TABLE; Schema: test; Owner: postgres
--

CREATE TABLE test.countries (
    id_contry character(2) NOT NULL,
    name_country character varying(40),
    id_region smallint
);


ALTER TABLE test.countries OWNER TO postgres;

--
-- Name: departments; Type: TABLE; Schema: test; Owner: postgres
--

CREATE TABLE test.departments (
    id_department smallint NOT NULL,
    name_department character varying(30),
    id_boss smallint,
    id_localization smallint
);


ALTER TABLE test.departments OWNER TO postgres;

--
-- Name: employees; Type: TABLE; Schema: test; Owner: postgres
--

CREATE TABLE test.employees (
    id_employee smallint NOT NULL,
    name character varying(20),
    surnames character varying(25),
    email character varying(25),
    phone character varying(20),
    hire_date timestamp without time zone,
    id_work character varying(10),
    salary numeric,
    pct_commission numeric,
    id_boss smallint,
    id_department smallint
);


ALTER TABLE test.employees OWNER TO postgres;

--
-- Name: localizations; Type: TABLE; Schema: test; Owner: postgres
--

CREATE TABLE test.localizations (
    id_localization smallint NOT NULL,
    address character varying(40),
    zip_code character varying(12),
    city character varying(30),
    state_province character varying(25),
    id_contry character(2)
);


ALTER TABLE test.localizations OWNER TO postgres;

--
-- Name: programmers; Type: VIEW; Schema: test; Owner: postgres
--

CREATE VIEW test.programmers AS
 SELECT employees.id_employee AS id,
    employees.id_boss,
    employees.name,
    employees.surnames,
    employees.email,
    employees.phone
   FROM test.employees
  WHERE ((employees.id_work)::text = 'IT_PROG'::text);


ALTER TABLE test.programmers OWNER TO postgres;

--
-- Name: work; Type: TABLE; Schema: test; Owner: postgres
--

CREATE TABLE test.work (
    id_work character varying(10) NOT NULL,
    name_work character varying(35),
    salary_min integer,
    salary_max integer
);


ALTER TABLE test.work OWNER TO postgres;

--
-- Name: programmers_bosses; Type: VIEW; Schema: test; Owner: postgres
--

CREATE VIEW test.programmers_bosses AS
 SELECT p.name,
    p.surnames,
    ee.salary,
    ec.name AS name_boss,
    ec.surnames AS surnames_boss,
    ec.salary AS salary_boss,
    t.name_work AS name_work_boss
   FROM (((test.employees ec
     JOIN test.programmers p ON ((p.id_boss = ec.id_employee)))
     JOIN test.work t ON (((t.id_work)::text = (ec.id_work)::text)))
     JOIN test.employees ee ON ((p.id = ee.id_employee)));


ALTER TABLE test.programmers_bosses OWNER TO postgres;

--
-- Name: regions; Type: TABLE; Schema: test; Owner: postgres
--

CREATE TABLE test.regions (
    id_region smallint NOT NULL,
    name_region character varying(25)
);


ALTER TABLE test.regions OWNER TO postgres;

--
-- Name: work_history; Type: TABLE; Schema: test; Owner: postgres
--

CREATE TABLE test.work_history (
    id_employee smallint NOT NULL,
    id_work character varying(10) NOT NULL,
    start_date timestamp without time zone,
    end_date timestamp without time zone,
    id_department smallint
);


ALTER TABLE test.work_history OWNER TO postgres;

--
-- Data for Name: categories; Type: TABLE DATA; Schema: test; Owner: postgres
--

COPY test.categories (category, lower_salary, higher_salary) FROM stdin;
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
A	500	2999
A	500	2999
A	500	2999
A	500	2999
A	500	2999
A	500	2999
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
B	3000	5999
C	6000	9999
D	10000	14999
E	15000	24999
F	25000	40000
A	500	2999
A	500	2999
A	500	2999
A	500	2999
A	500	2999
A	500	2999
\.


--
-- Data for Name: countries; Type: TABLE DATA; Schema: test; Owner: postgres
--

COPY test.countries (id_contry, name_country, id_region) FROM stdin;
CA	Canada	2
DE	Germany	1
UK	United Kingdom	1
US	United States of America	2
\.


--
-- Data for Name: departments; Type: TABLE DATA; Schema: test; Owner: postgres
--

COPY test.departments (id_department, name_department, id_boss, id_localization) FROM stdin;
10	Administration	200	1700
20	Marketing	201	1800
50	Shipping	124	1500
60	IT	103	1400
80	Sales	149	2500
90	Executive	100	1700
110	Accounting	205	1700
190	Contracting	\N	1700
145	RRHH	200	1700
\.


--
-- Data for Name: employees; Type: TABLE DATA; Schema: test; Owner: postgres
--

COPY test.employees (id_employee, name, surnames, email, phone, hire_date, id_work, salary, pct_commission, id_boss, id_department) FROM stdin;
101	Neena	Kochhar	NKOCHHAR	515.123.4568	1989-09-21 00:00:00	AD_VP	17000	\N	100	90
103	Alexander	Hunold	AHUNOLD	590.423.4567	1990-01-03 00:00:00	IT_PROG	9000	\N	102	60
104	Bruce	Ernst	BERNST	590.423.4568	1991-05-21 00:00:00	IT_PROG	6000	\N	103	60
107	Diana	Lorentz	DLORENTZ	590.423.5567	1999-02-07 00:00:00	IT_PROG	4200	\N	103	60
124	Kevin	Mourgos	KMOURGOS	650.123.5234	1999-11-16 00:00:00	ST_MAN	5800	\N	100	50
141	Trenna	Rajs	TRAJS	650.121.8009	1995-10-17 00:00:00	ST_CLERK	3500	\N	124	50
142	Curtis	Davies	CDAVIES	650.121.2994	1997-01-29 00:00:00	ST_CLERK	3100	\N	124	50
143	Randall	Matos	RMATOS	650.121.2874	1998-03-15 00:00:00	ST_CLERK	2600	\N	124	50
144	Peter	Vargas	PVARGAS	650.121.2004	1998-09-07 00:00:00	ST_CLERK	2500	\N	124	50
149	Eleni	Zlotkey	EZLOTKEY	011.44.1344.429018	2000-01-29 00:00:00	SA_MAN	10500	0.2	100	80
174	Ellen	Abel	EABEL	011.44.1644.429267	1996-05-11 00:00:00	SA_REP	11000	0.3	149	80
176	Jonathan	Taylor	JTAYLOR	011.44.1644.429265	1998-03-24 00:00:00	SA_REP	8600	0.2	149	80
178	Kimberely	Grant	KGRANT	011.44.1644.429263	1999-05-24 00:00:00	SA_REP	7000	0.15	149	\N
200	Jennifer	Whalen	JWHALEN	515.123.4444	1987-09-17 00:00:00	AD_ASST	4400	\N	101	10
201	Michael	Hartstein	MHARTSTE	515.123.5555	1996-02-17 00:00:00	MK_MAN	13000	\N	100	20
202	Pat	Fay	PFAY	603.123.6666	1997-08-17 00:00:00	MK_REP	6000	\N	201	20
205	Shelley	Higgins	SHIGGINS	515.123.8080	1994-06-07 00:00:00	AC_MGR	12000	\N	101	110
206	William	Gietz	WGIETZ	515.123.8181	1994-06-07 00:00:00	AC_ACCOUNT	8300	\N	205	110
100	Steven	King	SKING	515.123.4567	1987-06-17 00:00:00	AD_PRES	96000	\N	\N	90
102	Alex	De Haan	LDEHAAN	515.123.4569	1993-01-13 00:00:00	AD_VP	17000	\N	100	90
\.


--
-- Data for Name: localizations; Type: TABLE DATA; Schema: test; Owner: postgres
--

COPY test.localizations (id_localization, address, zip_code, city, state_province, id_contry) FROM stdin;
1400	2014 Jabberwocky Rd	26192	Southlake	Texas	US
1500	2001 Interiors Blvd	99236	South San Francisco	California	US
1700	2004 Charade Rd	98199	Seattle	Washington	US
1800	460 Bloor St. W.	ON M5S 1X8	Toronto	Ontario	CA
2500	Magdalen Centre, The Ofxord Science Park	OX9 9ZB	Oxford	Oxford	UK
\.


--
-- Data for Name: regions; Type: TABLE DATA; Schema: test; Owner: postgres
--

COPY test.regions (id_region, name_region) FROM stdin;
1	Europe
2	Americas
\.


--
-- Data for Name: work; Type: TABLE DATA; Schema: test; Owner: postgres
--

COPY test.work (id_work, name_work, salary_min, salary_max) FROM stdin;
AC_ACCOUNT	test Accountant	4200	9000
AC_MGR	Accounting Manager	8200	16000
AD_ASST	Administration Assistant	3000	6000
AD_PRES	President	20000	40000
AD_VP	Administration Vice President	15000	30000
IT_PROG	Programmer	4000	10000
MK_MAN	Marketing Manager	9000	15000
MK_REP	Marketing Representative	4000	9000
SA_MAN	Sales Manager	10000	20000
SA_REP	Sales Representative	6000	12000
ST_CLERK	Stock Clerk	2000	5000
ST_MAN	Stock Manager	5500	8500
\.


--
-- Data for Name: work_history; Type: TABLE DATA; Schema: test; Owner: postgres
--

COPY test.work_history (id_employee, id_work, start_date, end_date, id_department) FROM stdin;
101	AC_ACCOUNT	1989-09-21 00:00:00	1993-10-27 00:00:00	110
101	AC_MGR	1993-10-28 00:00:00	1997-03-15 00:00:00	110
102	IT_PROG	1993-01-13 00:00:00	1998-07-24 00:00:00	60
142	ST_CLERK	1999-01-01 00:00:00	1999-12-31 00:00:00	50
144	ST_CLERK	1998-03-24 00:00:00	1999-12-31 00:00:00	50
176	SA_MAN	1999-01-01 00:00:00	1999-12-31 00:00:00	80
176	SA_REP	1998-03-24 00:00:00	1998-12-31 00:00:00	80
200	AC_ACCOUNT	1994-07-01 00:00:00	1998-12-31 00:00:00	90
200	AD_ASST	1987-09-17 00:00:00	1993-06-17 00:00:00	90
201	MK_REP	1996-02-17 00:00:00	1999-12-19 00:00:00	20
\.


--
-- Name: countries countries_pk; Type: CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.countries
    ADD CONSTRAINT countries_pk PRIMARY KEY (id_contry);


--
-- Name: departments departament_pk; Type: CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.departments
    ADD CONSTRAINT departament_pk PRIMARY KEY (id_department);


--
-- Name: employees employees_pk; Type: CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.employees
    ADD CONSTRAINT employees_pk PRIMARY KEY (id_employee);


--
-- Name: localizations localizations_pk; Type: CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.localizations
    ADD CONSTRAINT localizations_pk PRIMARY KEY (id_localization);


--
-- Name: regions regions_pk; Type: CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.regions
    ADD CONSTRAINT regions_pk PRIMARY KEY (id_region);


--
-- Name: work_history work_history_pk; Type: CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.work_history
    ADD CONSTRAINT work_history_pk PRIMARY KEY (id_employee, id_work);


--
-- Name: work work_pk; Type: CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.work
    ADD CONSTRAINT work_pk PRIMARY KEY (id_work);


--
-- Name: programmers_bosses delete_pb; Type: RULE; Schema: test; Owner: postgres
--

CREATE RULE delete_pb AS
    ON DELETE TO test.programmers_bosses DO INSTEAD  DELETE FROM test.employees
  WHERE (((employees.name)::text = (old.name)::text) AND ((employees.surnames)::text = (old.surnames)::text));


--
-- Name: programmers ins_programmer; Type: RULE; Schema: test; Owner: postgres
--

CREATE RULE ins_programmer AS
    ON INSERT TO test.programmers
   WHERE (new.id IS NULL) DO INSTEAD  INSERT INTO test.employees (id_employee, name, surnames, email, phone, id_boss, id_department, id_work)
  VALUES (( SELECT (max(employees_1.id_employee) + 1)
           FROM test.employees employees_1), new.name, new.surnames, new.email, new.phone, 103, 60, 'IT_PROG'::character varying);


--
-- Name: programmers ins_programmer_default; Type: RULE; Schema: test; Owner: postgres
--

CREATE RULE ins_programmer_default AS
    ON INSERT TO test.programmers DO INSTEAD NOTHING;


--
-- Name: programmers_bosses update_pb; Type: RULE; Schema: test; Owner: postgres
--

CREATE RULE update_pb AS
    ON UPDATE TO test.programmers_bosses DO INSTEAD  UPDATE test.employees SET name = new.name, surnames = new.surnames, salary = new.salary
  WHERE (((employees.name)::text = (new.name)::text) AND ((employees.surnames)::text = (new.surnames)::text));


--
-- Name: countries countries_regions_fk; Type: FK CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.countries
    ADD CONSTRAINT countries_regions_fk FOREIGN KEY (id_region) REFERENCES test.regions(id_region);


--
-- Name: departments departments_cap_fk; Type: FK CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.departments
    ADD CONSTRAINT departments_cap_fk FOREIGN KEY (id_boss) REFERENCES test.employees(id_employee);


--
-- Name: departments departments_localizations_fk; Type: FK CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.departments
    ADD CONSTRAINT departments_localizations_fk FOREIGN KEY (id_localization) REFERENCES test.localizations(id_localization);


--
-- Name: employees employees_work_fk; Type: FK CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.employees
    ADD CONSTRAINT employees_work_fk FOREIGN KEY (id_work) REFERENCES test.work(id_work);


--
-- Name: localizations localizations_countries_fk; Type: FK CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.localizations
    ADD CONSTRAINT localizations_countries_fk FOREIGN KEY (id_contry) REFERENCES test.countries(id_contry);


--
-- Name: work_history work_history_departments_fk; Type: FK CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.work_history
    ADD CONSTRAINT work_history_departments_fk FOREIGN KEY (id_department) REFERENCES test.departments(id_department);


--
-- Name: work_history work_history_work_fk; Type: FK CONSTRAINT; Schema: test; Owner: postgres
--

ALTER TABLE ONLY test.work_history
    ADD CONSTRAINT work_history_work_fk FOREIGN KEY (id_work) REFERENCES test.work(id_work);


--
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: postgres
--

GRANT ALL ON SCHEMA public TO PUBLIC;


--
-- PostgreSQL database dump complete
--

