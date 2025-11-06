// Handles player creation management and validation

$(document).ready(function () {
    const REQUIRED_POSITIONS = { GK: 2, DF: 8, MD: 8, AT: 5 };
    const MAX_PLAYERS = 23;
    const GENERATE_PLAYERS_URL = $('#generatePlayersUrl').data('url') || '/Representative/GeneratePlayers';
    const POSITIONS = ['GK', 'DF', 'MD', 'AT'];
    const $tbody = $('#playersTableBody');
    const $error = $('#playersError');
    const $header = $('#playersHeader');

    function getPlayerCount() {
        return Math.max(0, $tbody.find('tr').length - 1); // Subtract 1 for addRow
    }

    function updateHeader() {
        const count = getPlayerCount();
        $header.text(`Players (${count} / ${MAX_PLAYERS})`);
        $('#addRow').toggle(count < MAX_PLAYERS);
    }

    function showError(message) {
        $error.text(message).show();
        const offset = $error.offset();
        if (offset) {
            $('html, body').animate({ scrollTop: offset.top - 100 }, 300);
        }
        setTimeout(() => $error.fadeOut(300), 3000);
    }

    // Generate ratings based on natural position
    function getRatings(pos) {
        const rnd = (min, max) => Math.floor(Math.random() * (max - min + 1)) + min;
        return {
            GK: pos === 'GK' ? rnd(50, 100) : rnd(0, 50),
            DF: pos === 'DF' ? rnd(50, 100) : rnd(0, 50),
            MD: pos === 'MD' ? rnd(50, 100) : rnd(0, 50),
            AT: pos === 'AT' ? rnd(50, 100) : rnd(0, 50)
        };
    }

    // Create player row HTML
    function createPlayerRow(index, player) {
        const id = player.id || player.Id || crypto.randomUUID();
        const name = player.name || player.Name || '';
        const pos = player.naturalPosition || player.NaturalPosition || 'DF';
        const ratings = player.ratings || player.Ratings || getRatings(pos);
        const r = { GK: ratings.gk ?? ratings.GK ?? 0, DF: ratings.df ?? ratings.DF ?? 0, 
                    MD: ratings.md ?? ratings.MD ?? 0, AT: ratings.at ?? ratings.AT ?? 0 };
        
        const options = POSITIONS.map(p => `<option value="${p}" ${pos === p ? 'selected' : ''}>${p}</option>`).join('');
        
        return $(`
            <tr data-player-index="${index}">
                <td>
                    <input type="hidden" name="Players[${index}].Id" value="${id}" />
                    <input type="hidden" class="natpos-hidden" name="Players[${index}].NaturalPosition" value="${pos}" />
                    <input type="text" name="Players[${index}].Name" value="${name}" class="form-control" required />
                </td>
                <td>
                    <select class="form-select natural-pos-select" data-name-prefix="Players[${index}]" required>
                        ${options}
                    </select>
                </td>
                <td><input type="number" name="Players[${index}].Ratings.GK" value="${r.GK}" class="form-control" min="0" max="100" required readonly /></td>
                <td><input type="number" name="Players[${index}].Ratings.DF" value="${r.DF}" class="form-control" min="0" max="100" required readonly /></td>
                <td><input type="number" name="Players[${index}].Ratings.MD" value="${r.MD}" class="form-control" min="0" max="100" required readonly /></td>
                <td><input type="number" name="Players[${index}].Ratings.AT" value="${r.AT}" class="form-control" min="0" max="100" required readonly /></td>
                <td class="captain-cell">
                    <input type="radio" class="form-check-input captain-radio" name="CaptainId" value="${id}" />
                </td>
                <td class="actions-cell">
                    <button type="button" class="btn btn-sm btn-outline-danger remove-player-btn">Remove</button>
                </td>
            </tr>
        `);
    }

    // Reindex form inputs after row removal
    function reindexRows() {
        $tbody.find('tr[data-player-index]').each(function(i) {
            const $row = $(this);
            $row.attr('data-player-index', i);
            $row.find('input, select').each(function() {
                const name = $(this).attr('name');
                if (name) $(this).attr('name', name.replace(/Players\[\d+\]/, `Players[${i}]`));
            });
            $row.find('.natural-pos-select').attr('data-name-prefix', `Players[${i}]`);
        });
        if (!$('#addRow').length) {
            $tbody.append('<tr id="addRow"><td colspan="7" class="text-center text-muted">Add until there are 23 players.</td><td class="actions-cell"><button type="button" class="btn btn-sm btn-outline-primary" id="addPlayerBtn">Add</button></td></tr>');
        }
        $('#addRow').appendTo($tbody);
    }

    // Generate remaining players to fill squad
    $('#generateRandomBtn').click(function() {
        const $btn = $(this);
        $btn.prop('disabled', true).text('Generating...');

        const existing = [];
        $tbody.find('tr[data-player-index]').each(function() {
            const $tr = $(this);
            const id = $tr.find('input[name$=".Id"]').val();
            const name = $tr.find('input[name$=".Name"]').val();
            const pos = $tr.find('.natural-pos-select').val();
            if (id && name && pos) existing.push({ id, name, naturalPosition: pos });
        });

        $.ajax({
            url: GENERATE_PLAYERS_URL,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ players: existing }),
            success: function(response) {
                if (response.success && response.players) {
                    const startIdx = getPlayerCount();
                    response.players.forEach((p, i) => {
                        $('#addRow').before(createPlayerRow(startIdx + i, p));
                    });
                    $error.hide();
                    updateHeader();
                } else {
                    showError(response.message || 'Failed to generate players');
                }
            },
            error: () => showError('An error occurred while generating players.'),
            complete: () => $btn.prop('disabled', false).html('<i class="bi bi-shuffle"></i> Generate Random Team')
        });
    });

    // Add new player row
    $(document).on('click', '#addPlayerBtn', function() {
        if (getPlayerCount() >= MAX_PLAYERS) return;
        $('#addRow').before(createPlayerRow(getPlayerCount(), { naturalPosition: 'DF' }));
        updateHeader();
    });

    // Remove player row
    $tbody.on('click', '.remove-player-btn', function() {
        $(this).closest('tr').remove();
        reindexRows();
        updateHeader();
    });

    // Update ratings when position changes
    $tbody.on('change', '.natural-pos-select', function() {
        const $tr = $(this).closest('tr');
        const idx = $tr.attr('data-player-index');
        const pos = $(this).val();
        $tr.find('.natpos-hidden').val(pos);
        const r = getRatings(pos);
        ['GK', 'DF', 'MD', 'AT'].forEach(rating => {
            $tr.find(`input[name^="Players[${idx}].Ratings.${rating}"]`).val(r[rating]);
        });
    });

    // Form validation
    $('#createTeamForm').submit(function(e) {
        const count = getPlayerCount();
        const counts = { GK: 0, DF: 0, MD: 0, AT: 0 };
        
        $tbody.find('tr[data-player-index]').each(function() {
            const pos = $(this).find('.natural-pos-select').val() || $(this).find('.natpos-hidden').val();
            if (counts.hasOwnProperty(pos)) counts[pos]++;
        });

        // Validate player count
        if (count !== MAX_PLAYERS) {
            e.preventDefault();
            showError(`Team must have exactly ${MAX_PLAYERS} players.`);
            return false;
        }

        // Validate captain selection
        if (!$('input[name="CaptainId"]:checked').length) {
            e.preventDefault();
            alert('Please select a captain.');
            return false;
        }

        // Validate position distribution
        if (counts.GK < REQUIRED_POSITIONS.GK || counts.DF < REQUIRED_POSITIONS.DF || 
            counts.MD < REQUIRED_POSITIONS.MD || counts.AT < REQUIRED_POSITIONS.AT) {
            e.preventDefault();
            showError(`Squad must include at least ${REQUIRED_POSITIONS.GK} GK, ${REQUIRED_POSITIONS.DF} DF, ${REQUIRED_POSITIONS.MD} MD, and ${REQUIRED_POSITIONS.AT} AT.`);
            return false;
        }
    });

    updateHeader();
});
