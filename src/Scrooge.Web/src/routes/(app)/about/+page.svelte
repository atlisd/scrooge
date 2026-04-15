<script lang="ts">
  import pkg from '../../../../package.json';
  import { logout } from '$lib/api';
  import { activeUser, setActiveUser, getCachedUsers } from '$lib/user';

  const users = getCachedUsers();
</script>

<div class="card shadow-sm">
  <div class="card-body">
    <h3 class="card-title mb-3">About Scrooge</h3>
    <p class="mb-1"><strong>Version:</strong> {pkg.version}</p>
    <p class="mb-3">
      <a href="https://github.com/atlisd/scrooge" target="_blank" rel="noopener noreferrer">
        View on GitHub
      </a>
    </p>

    {#if users.length > 1}
      <div class="mb-3">
        <p class="mb-1"><strong>Active user:</strong></p>
        {#each users as user}
          <div class="form-check">
            <input
              class="form-check-input"
              type="radio"
              id="user-{user.id}"
              name="activeUser"
              checked={$activeUser?.id === user.id}
              onchange={() => setActiveUser(user)}
            />
            <label class="form-check-label" for="user-{user.id}">{user.name}</label>
          </div>
        {/each}
      </div>
    {/if}

    <button class="btn btn-outline-danger btn-sm" onclick={logout}>Logout</button>
  </div>
</div>
