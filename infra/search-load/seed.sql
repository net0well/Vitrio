\set ON_ERROR_STOP on

begin;

create temp table load_professions (category_id int primary key, headline text, service text);
insert into load_professions values
  (1, 'Eletricista', 'Instalação de chuveiro'),
  (2, 'Encanador', 'Conserto de vazamento'),
  (3, 'Pedreiro', 'Reboco e contrapiso'),
  (4, 'Pintor', 'Pintura de parede'),
  (5, 'Diarista', 'Faxina completa'),
  (6, 'Jardineiro', 'Corte de grama'),
  (7, 'Montador de móveis', 'Montagem de guarda-roupa'),
  (8, 'Técnico de ar-condicionado', 'Limpeza de split'),
  (9, 'Marido de aluguel', 'Pequenos reparos'),
  (10, 'Fretes e mudanças', 'Mudança residencial'),
  (11, 'Cabeleireira a domicílio', 'Corte e escova'),
  (12, 'Professor particular', 'Aula de matemática'),
  (13, 'Técnico de informática', 'Formatação de computador'),
  (14, 'Cuidadora de idosos', 'Acompanhamento diurno'),
  (15, 'Banho e tosa', 'Banho em cães');

create temp table load_cities as
select ibge_code,
       st_y(centroid::geometry) as latitude,
       st_x(centroid::geometry) as longitude,
       sum(greatest(population, 1)) over (order by ibge_code) as cumulative,
       sum(greatest(population, 1)) over () as total
from cities
where state = 'SP';

create temp table load_providers as
select gen_random_uuid() as provider_id,
       gen_random_uuid() as user_id,
       draw.n,
       city.ibge_code,
       round(((city.latitude + (random() - 0.5) * 0.18) / 0.0045)::numeric) * 0.0045 as latitude,
       round(((city.longitude + (random() - 0.5) * 0.18) / 0.0045)::numeric) * 0.0045 as longitude,
       1 + floor(random() * 15)::int as category_id,
       (array['Ana','Bruno','Carla','Diego','Elaine','Fábio','Gisele','Hugo','Irene','João','Karina','Luiz','Márcia','Nelson','Olívia','Paulo','Quésia','Rafael','Sônia','Tiago'])[1 + floor(random() * 20)::int]
         || ' ' ||
       (array['Silva','Santos','Oliveira','Souza','Lima','Pereira','Costa','Ferreira','Almeida','Ribeiro','Martins','Rocha','Gomes','Barbosa','Moreira'])[1 + floor(random() * 15)::int] as display_name,
       5 + floor(random() * 26)::int as radius_km,
       random() < 0.6 as rated,
       random() < 0.15 as available_today
from (select n, random() as pick from generate_series(1, 50000) n) draw
cross join lateral (
  select * from load_cities c where c.cumulative >= draw.pick * c.total order by c.cumulative limit 1) city;

insert into users (id, email, display_name, password_hash, password_changed_at, status, accepted_terms_version,
                   terms_accepted_at, created_at, failed_sign_in_count)
select user_id, 'carga-' || n || '@carga.vitrio.local', display_name, 'carga', now(), 'Active', 'carga', now(), now(), 0
from load_providers;

insert into providers (id, user_id, display_name, headline, about, years_of_experience, whats_app_enabled, status,
                       published_at, city_ibge_code, public_latitude, public_longitude, location_approximate,
                       service_radius_meters, available_today, available_today_set_at, handles_emergencies,
                       rating_score, review_count, completed_orders, response_sample_size, late_cancellations90d,
                       created_at, updated_at, search_primary, search_name, search_secondary, search_terms)
select lp.provider_id,
       lp.user_id,
       lp.display_name,
       profession.headline,
       'Trabalho com ' || lower(profession.headline) || ' há anos, com capricho, pontualidade e garantia no serviço feito.',
       case when random() < 0.7 then 1 + floor(random() * 30)::int end,
       false,
       'Published',
       now(),
       lp.ibge_code,
       lp.latitude,
       lp.longitude,
       false,
       lp.radius_km * 1000,
       lp.available_today,
       case when lp.available_today then now() end,
       random() < 0.2,
       case when lp.rated then round((3.5 + random() * 1.5)::numeric, 2) end,
       case when lp.rated then 3 + floor(random() * 150)::int else floor(random() * 3)::int end,
       0, 0, 0,
       now(), now(),
       lower(unaccent(profession.headline || ' ' || category.name || ' ' || profession.service || ' Visita técnica')),
       lower(unaccent(lp.display_name)),
       lower(unaccent('Trabalho com ' || profession.headline || ' há anos, com capricho, pontualidade e garantia no serviço feito.')),
       lower(unaccent(profession.headline || ' ' || category.name || ' ' || profession.service || ' Visita técnica ' || lp.display_name))
from load_providers lp
join load_professions profession on profession.category_id = lp.category_id
join categories category on category.id = lp.category_id;

insert into provider_categories (id, provider_id, category_id, is_primary)
select gen_random_uuid(), provider_id, category_id, true from load_providers;

insert into provider_services (id, provider_id, title, price_from_cents, price_unit, on_quote, sort_order)
select gen_random_uuid(), lp.provider_id, profession.service, (50 + floor(random() * 450)::int) * 100, 'PerService', false, 0
from load_providers lp join load_professions profession on profession.category_id = lp.category_id;

insert into provider_services (id, provider_id, title, price_from_cents, price_unit, on_quote, sort_order)
select gen_random_uuid(), provider_id, 'Visita técnica', (40 + floor(random() * 80)::int) * 100, 'PerHour', false, 1
from load_providers;

commit;

analyze users;
analyze providers;
analyze provider_categories;
analyze provider_services;

select count(*) as published_providers from providers where status = 'Published';
