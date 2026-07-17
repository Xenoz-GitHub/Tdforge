-- TdForge - Supabase Schema

-- Projects table
create table if not exists public.projects (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references auth.users(id) on delete cascade,
  name text not null default 'Untitled',
  code text not null default '',
  scene jsonb default '[]'::jsonb,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

-- Index for fast user project listing
create index if not exists idx_projects_user_id on public.projects(user_id);
create index if not exists idx_projects_updated_at on public.projects(updated_at desc);

-- Auto-update updated_at on row change
create or replace function public.handle_updated_at()
returns trigger as $$
begin
  new.updated_at = now();
  return new;
end;
$$ language plpgsql security definer;

create trigger trg_projects_updated_at
  before update on public.projects
  for each row execute function public.handle_updated_at();

-- Row-Level Security
alter table public.projects enable row level security;

-- Users can only see their own projects
create policy "Users can view their own projects"
  on public.projects for select
  using (auth.uid() = user_id);

-- Users can create their own projects
create policy "Users can create projects"
  on public.projects for insert
  with check (auth.uid() = user_id);

-- Users can update their own projects
create policy "Users can update their own projects"
  on public.projects for update
  using (auth.uid() = user_id);

-- Users can delete their own projects
create policy "Users can delete their own projects"
  on public.projects for delete
  using (auth.uid() = user_id);

-- Templates table (shared, read-only)
create table if not exists public.templates (
  id uuid primary key default gen_random_uuid(),
  name text not null,
  description text default '',
  code text not null,
  category text not null default 'demo',
  created_at timestamptz not null default now()
);

alter table public.templates enable row level security;

create policy "Anyone can view templates"
  on public.templates for select
  using (true);

create policy "Only authenticated users can insert templates"
  on public.templates for insert
  with check (auth.role() = 'authenticated');
